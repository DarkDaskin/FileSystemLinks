using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace FileSystemLinks;

internal partial class WindowsFileSystem : IFileSystem
{
    public void CreateHardLink(string sourceFileName, string destFileName)
    {
        if (!CreateHardLinkNative(destFileName, sourceFileName, IntPtr.Zero))
            ThrowExceptionForLastError(false);
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
            ThrowExceptionForLastError(isDirectory);
    }

    public string? GetLinkTarget(string linkPath, bool isDirectory) =>
        GetImmediateLinkTarget(linkPath, isDirectory, false, false);

    /// <summary>
    /// Gets reparse point information associated to <paramref name="linkPath"/>.
    /// </summary>
    /// <returns>The immediate link target, absolute or relative or null if the file is not a supported link.</returns>
    private static string? GetImmediateLinkTarget(string linkPath, bool isDirectory, bool throwOnError, bool returnFullPath)
    {
        using var handle = OpenSafeFileHandle(linkPath, CreateFileFlags.FILE_FLAG_BACKUP_SEMANTICS | CreateFileFlags.FILE_FLAG_OPEN_REPARSE_POINT);

        if (handle.IsInvalid)
        {
            if (throwOnError)
                ThrowExceptionForLastError(isDirectory);

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
                    ThrowExceptionForLastError(isDirectory);

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

    private static SafeFileHandle OpenSafeFileHandle(string path, CreateFileFlags flags)
    {
        path = PathInternal.EnsureExtendedPrefixIfNeeded(path);

        return CreateFileNative(path, 0,
            FileShare.ReadWrite | FileShare.Delete, IntPtr.Zero, FileMode.Open, (uint)flags);
    }

    private static void ThrowExceptionForLastError(bool isDirectory) =>
        ThrowExceptionForWin32Error(Marshal.GetLastWin32Error(), isDirectory);

    private static void ThrowExceptionForWin32Error(int errorCode, bool isDirectory)
    {
        try
        {
            Marshal.ThrowExceptionForHR(HRESULT_HIGH | errorCode);
        }
        catch (COMException exception)
        {
            throw new IOException(exception.Message, exception.HResult);
        }
        catch (FileNotFoundException) when (isDirectory)
        {
            throw new DirectoryNotFoundException();
        }
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

    // https://docs.microsoft.com/windows-hardware/drivers/ifs/fsctl-get-reparse-point
    // ReSharper disable InconsistentNaming
    private const int MAXIMUM_REPARSE_DATA_BUFFER_SIZE = 16 * 1024;

    private const uint FSCTL_GET_REPARSE_POINT = 0x000900A8;

    private const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
    private const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;
    private const uint SYMLINK_FLAG_RELATIVE = 1;

    private const int ERROR_NOT_A_REPARSE_POINT = 0x1126;

    private const int HRESULT_HIGH = unchecked((int)0x80070000);
    // ReSharper restore InconsistentNaming
}