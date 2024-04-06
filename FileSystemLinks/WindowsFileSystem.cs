using System;
#if NETSTANDARD2_1_OR_GREATER
using System.Buffers;
#endif
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32.SafeHandles;

namespace FileSystemLinks;

internal partial class WindowsFileSystem : IFileSystem
{
    public void CreateHardLink(string sourceFileName, string destFileName)
    {
        if (!CreateHardLinkNative(destFileName, sourceFileName, IntPtr.Zero))
            throw GetExceptionForLastWin32Error(false);
    }
    
    public void CreateSymbolicLink(string path, string pathToTarget, bool isDirectory)
    {
        path = PathInternal.EnsureExtendedPrefixIfNeeded(path);
        pathToTarget = PathInternal.EnsureExtendedPrefixIfNeeded(pathToTarget);

        CreateSymbolicLinkFlags flags = 0;
        if (isDirectory)
            flags |= CreateSymbolicLinkFlags.SYMBOLIC_LINK_FLAG_DIRECTORY;
        if (Environment.OSVersion.Version is { Major: >= 11 } or { Major: 10, Build: >= 14972 })
            flags |= CreateSymbolicLinkFlags.SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE;

        if (!CreateSymbolicLinkNative(path, pathToTarget, flags))
            throw GetExceptionForLastWin32Error(isDirectory);
    }

    public string? GetLinkTarget(string linkPath, bool isDirectory) =>
        GetImmediateLinkTarget(linkPath, isDirectory, throwOnError: false, returnFullPath: false);

    public string? ResolveLinkTarget(string linkPath, bool returnFinalTarget, bool isDirectory)
    {
        return returnFinalTarget
            ? GetFinalLinkTarget(linkPath, isDirectory)
            : GetImmediateLinkTarget(linkPath, isDirectory, throwOnError: true, returnFullPath: true);
    }

    /// <summary>
    /// Gets reparse point information associated to <paramref name="linkPath"/>.
    /// </summary>
    /// <returns>The immediate link target, absolute or relative or null if the file is not a supported link.</returns>
    private static string? GetImmediateLinkTarget(string linkPath, bool isDirectory, bool throwOnError, bool returnFullPath)
    {
        var linkPathExtended = PathInternal.EnsureExtendedPrefixIfNeeded(linkPath);
        using var handle = OpenSafeFileHandle(linkPathExtended, CreateFileFlags.FILE_FLAG_BACKUP_SEMANTICS | CreateFileFlags.FILE_FLAG_OPEN_REPARSE_POINT);

        if (handle.IsInvalid)
        {
            if (throwOnError)
                throw GetExceptionForLastWin32Error(isDirectory);

            return null;
        }

        var buffer = Marshal.AllocHGlobal(MAXIMUM_REPARSE_DATA_BUFFER_SIZE);
        try
        {
            var success = DeviceIoControlNative(handle, FSCTL_GET_REPARSE_POINT, IntPtr.Zero, 0, buffer, MAXIMUM_REPARSE_DATA_BUFFER_SIZE,
                out _, IntPtr.Zero);
            if (!success)
            {
                var errorCode = Marshal.GetLastWin32Error();

                if (throwOnError && errorCode != ERROR_NOT_A_REPARSE_POINT)
                {
                    throw GetExceptionForLastWin32Error(isDirectory);
                }

                return null;
            }

            var reparseHeader = Marshal.PtrToStructure<ReparseHeader>(buffer);
            var offset = Marshal.SizeOf<ReparseHeader>();
            
            // We always use SubstituteName(Offset|Length) instead of PrintName(Offset|Length),
            // the latter is just the display name of the reparse point and it can show something completely unrelated to the target.

            if (reparseHeader.ReparseTag == IO_REPARSE_TAG_SYMLINK)
            {
                var reparseData = Marshal.PtrToStructure<SymbolicLinkData>(buffer + offset);
                offset += Marshal.SizeOf<SymbolicLinkData>() + reparseData.SubstituteNameOffset;
                var targetPath = Marshal.PtrToStringUni(buffer + offset, reparseData.SubstituteNameLength / 2);
                var isRelative = (reparseData.Flags & SYMLINK_FLAG_RELATIVE) != 0;

                if (!isRelative)
                {
                    if (targetPath.StartsWith(PathInternal.UncNTPathPrefix, StringComparison.Ordinal))
                        return Path.Combine(PathInternal.UncPathPrefix, targetPath.Substring(PathInternal.UncNTPathPrefix.Length));

                    return GetTargetPathWithoutNTPrefix(targetPath);
                }
                else if (returnFullPath)
                    return Path.Combine(Path.GetDirectoryName(linkPath)!, targetPath);
                else
                    return targetPath;
            }
            else if (reparseHeader.ReparseTag == IO_REPARSE_TAG_MOUNT_POINT)
            {
                var reparseData = Marshal.PtrToStructure<JunctionData>(buffer + offset);
                offset += Marshal.SizeOf<JunctionData>() + reparseData.SubstituteNameOffset;
                var targetPath = Marshal.PtrToStringUni(buffer + offset, reparseData.SubstituteNameLength);

                // Unlike symbolic links, mount point paths cannot be relative.
                Debug.Assert(!PathInternal.IsPartiallyQualified(targetPath));
                // Mount points cannot point to a remote location.
                Debug.Assert(!targetPath.StartsWith(PathInternal.UncNTPathPrefix));
                return GetTargetPathWithoutNTPrefix(targetPath);
            }

            return null;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        // ReSharper disable once InconsistentNaming
        static string GetTargetPathWithoutNTPrefix(string targetPath)
        {
            Debug.Assert(targetPath.StartsWith(PathInternal.NTPathPrefix));
            return targetPath.Substring(PathInternal.NTPathPrefix.Length);
        }
    }

    private string? GetFinalLinkTarget(string linkPath, bool isDirectory)
    {
        var linkPathExtended = PathInternal.EnsureExtendedPrefixIfNeeded(linkPath);

        GetFindData(linkPathExtended, isDirectory, out var findData);

        // The file or directory is not a reparse point.
        if ((findData.dwFileAttributes & (uint)FileAttributes.ReparsePoint) == 0 ||
            // Only symbolic links and mount points are supported at the moment.
            (findData.dwReserved0 != IO_REPARSE_TAG_SYMLINK &&
             findData.dwReserved0 != IO_REPARSE_TAG_MOUNT_POINT))
        {
            return null;
        }

        // We try to open the final file since they asked for the final target.
        using var handle = OpenSafeFileHandle(linkPath,CreateFileFlags.FILE_FLAG_BACKUP_SEMANTICS);

        if (handle.IsInvalid)
        {
            // If the handle fails because it is unreachable, is because the link was broken.
            // We need to fallback to manually traverse the links and return the target of the last resolved link.
            int error = Marshal.GetLastWin32Error();
            if (IsPathUnreachableError(error))
                return GetFinalLinkTargetSlow(linkPath, isDirectory);

            throw GetExceptionForWin32Error(error, isDirectory);
        }

        const int initialBufferSize = 4096;
#if NETSTANDARD2_1_OR_GREATER
        var buffer = ArrayPool<char>.Shared.Rent(initialBufferSize);
        try
#else
        var buffer = new char[initialBufferSize];
#endif
        {
            var length = GetFinalPathNameByHandle(handle, buffer);

            // If the function fails because lpszFilePath is too small to hold the string plus the terminating null character,
            // the return value is the required buffer size, in TCHARs. This value includes the size of the terminating null character.
            if (length > buffer.Length)
            {
#if NETSTANDARD2_1_OR_GREATER
                ArrayPool<char>.Shared.Return(buffer);
                buffer = ArrayPool<char>.Shared.Rent(length);
#else
                buffer = new char[length];
#endif

                length = GetFinalPathNameByHandle(handle, buffer);
            }

            // If the function fails for any other reason, the return value is zero.
            if (length == 0)
                throw GetExceptionForLastWin32Error(isDirectory);

            Debug.Assert(PathInternal.IsExtended(new string(buffer, 0, length)));
            // GetFinalPathNameByHandle always returns with extended DOS prefix even if the link target was created without one.
            // While this does not interfere with correct behavior, it might be unexpected.
            // Hence we trim it if the passed-in path to the link wasn't extended.
            var start = PathInternal.IsExtended(linkPath) ? 0 : 4;
            return new string(buffer, start, length - start);
        }
#if NETSTANDARD2_1_OR_GREATER
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
#endif
    }

    private static string? GetFinalLinkTargetSlow(string linkPath, bool isDirectory)
    {
        // We don't throw on error since we already did all the proper validations before.
        string? current = GetImmediateLinkTarget(linkPath, isDirectory, throwOnError: false, returnFullPath: true);
        string? prev = null;

        while (current != null)
        {
            prev = current;
            current = GetImmediateLinkTarget(current, isDirectory, throwOnError: false, returnFullPath: true);
        }

        return prev;
    }

    private static int GetFinalPathNameByHandle(SafeFileHandle handle, char[] buffer) =>
        GetFinalPathNameByHandleNative(handle, buffer, buffer.Length, 0);


    private static void GetFindData(string fullPath, bool isDirectory, out WIN32_FIND_DATA findData)
    {
        using var handle = FindFirstFileNative(PathInternal.TrimEndingDirectorySeparator(fullPath), out findData);
        if (handle.IsInvalid)
        {
            throw GetExceptionForLastWin32Error(isDirectory);
        }
    }

    private static SafeFileHandle OpenSafeFileHandle(string path, CreateFileFlags flags) => CreateFileNative(path, 0,
            FileShare.ReadWrite | FileShare.Delete, IntPtr.Zero, FileMode.Open, (uint)flags);

    internal static bool IsPathUnreachableError(int errorCode)
    {
        switch (errorCode)
        {
            case ERROR_FILE_NOT_FOUND:
            case ERROR_PATH_NOT_FOUND:
            case ERROR_NOT_READY:
            case ERROR_INVALID_NAME:
            case ERROR_BAD_PATHNAME:
            case ERROR_BAD_NETPATH:
            case ERROR_BAD_NET_NAME:
            case ERROR_INVALID_PARAMETER:
            case ERROR_NETWORK_UNREACHABLE:
            case ERROR_NETWORK_ACCESS_DENIED:
            case ERROR_INVALID_HANDLE:           // eg from \\.\CON
            case ERROR_FILENAME_EXCED_RANGE:     // Path is too long
                return true;
            default:
                return false;
        }
    }
    private static Exception GetExceptionForLastWin32Error(bool isDirectory) =>
        GetExceptionForWin32Error(Marshal.GetLastWin32Error(), isDirectory);

    private static Exception GetExceptionForWin32Error(int errorCode, bool isDirectory)
    {
        var exception = Marshal.GetExceptionForHR(HRESULT_HIGH | errorCode);
        if (exception is COMException)
            return new IOException(exception.Message, exception.HResult);
        if (exception is FileNotFoundException && isDirectory)
            return new DirectoryNotFoundException();
        return exception;
    }

    [DllImport("kernel32", EntryPoint = "CreateHardLinkW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateHardLinkNative(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

    [DllImport("kernel32", EntryPoint = "CreateSymbolicLinkW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateSymbolicLinkNative(string lpSymlinkFileName, string lpTargetFileName, CreateSymbolicLinkFlags dwFlags);

    [DllImport("kernel32", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFileNative(string lpFileName, FileAccess dwDesiredAccess, FileShare dwShareMode,
        IntPtr lpSecurityAttributes, FileMode dwCreationDisposition, uint dwFlagsAndAttributes);

    [DllImport("kernel32", EntryPoint = "DeviceIoControl", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool DeviceIoControlNative(SafeFileHandle hDevice, uint dwIoControlCode, IntPtr lpInBuffer,
        int nInBufferSize, IntPtr lpOutBuffer, int nOutBufferSize, out int lpBytesReturned, IntPtr lpOverlapped);

    [DllImport("kernel32", EntryPoint = "FindFirstFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFindHandle FindFirstFileNative(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

    [DllImport("kernel32", EntryPoint = "FindClose", SetLastError = true)]
    private static extern bool FindCloseNative(IntPtr hFindFile);

    [DllImport("kernel32", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetFinalPathNameByHandleNative(SafeFileHandle hFile, [Out] char[] lpszFilePath, int cchFilePath, uint dwFlags);

    [Flags]
    private enum CreateSymbolicLinkFlags : uint
    {
        // ReSharper disable InconsistentNaming
        SYMBOLIC_LINK_FLAG_DIRECTORY = 1 << 0,
        SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE = 1 << 1,
        // ReSharper restore InconsistentNaming
    }

    [Flags]
    private enum CreateFileFlags : uint
    {
        // ReSharper disable InconsistentNaming
        FILE_FLAG_BACKUP_SEMANTICS = 0x02000000,
        FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000,
        // ReSharper restore InconsistentNaming
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ReparseHeader
    {
        public uint ReparseTag;
        public ushort ReparseDataLength;
        public ushort Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SymbolicLinkData
    {
        public ushort SubstituteNameOffset;
        public ushort SubstituteNameLength;
        public ushort PrintNameOffset;
        public ushort PrintNameLength;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JunctionData
    {
        public ushort SubstituteNameOffset;
        public ushort SubstituteNameLength;
        public ushort PrintNameOffset;
        public ushort PrintNameLength;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    // ReSharper disable once InconsistentNaming
    private struct WIN32_FIND_DATA
    {
        internal uint dwFileAttributes;
        internal FILETIME ftCreationTime;
        internal FILETIME ftLastAccessTime;
        internal FILETIME ftLastWriteTime;
        internal uint nFileSizeHigh;
        internal uint nFileSizeLow;
        internal uint dwReserved0;
        internal uint dwReserved1;
        private FileName _cFileName;
        private AlternateFileName _cAlternateFileName;

        [StructLayout(LayoutKind.Explicit, Size = 260 * sizeof(char))]
        private struct FileName { }

        [StructLayout(LayoutKind.Explicit, Size = 14 * sizeof(char))]
        private struct AlternateFileName { }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeFindHandle() : base(true) { }

        public SafeFindHandle(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle() => FindCloseNative(handle);
    }

    // https://docs.microsoft.com/windows-hardware/drivers/ifs/fsctl-get-reparse-point
    // ReSharper disable InconsistentNaming
    private const int MAXIMUM_REPARSE_DATA_BUFFER_SIZE = 16 * 1024;

    private const uint FSCTL_GET_REPARSE_POINT = 0x000900A8;

    private const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
    private const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;
    private const uint SYMLINK_FLAG_RELATIVE = 1;

    private const int ERROR_FILE_NOT_FOUND = 0x2;
    private const int ERROR_PATH_NOT_FOUND = 0x3;
    private const int ERROR_INVALID_HANDLE = 0x6;
    private const int ERROR_NOT_READY = 0x15;
    private const int ERROR_BAD_NETPATH = 0x35;
    private const int ERROR_NETWORK_ACCESS_DENIED = 0x41;
    private const int ERROR_BAD_NET_NAME = 0x43;
    private const int ERROR_INVALID_PARAMETER = 0x57;
    private const int ERROR_INVALID_NAME = 0x7B;
    private const int ERROR_BAD_PATHNAME = 0xA1;
    private const int ERROR_FILENAME_EXCED_RANGE = 0xCE;
    private const int ERROR_NETWORK_UNREACHABLE = 0x4CF;
    private const int ERROR_NOT_A_REPARSE_POINT = 0x1126;

    private const int HRESULT_HIGH = unchecked((int)0x80070000);
    // ReSharper restore InconsistentNaming
}