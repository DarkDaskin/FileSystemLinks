using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FileSystemLinks;

internal partial class WindowsFileSystem : IFileSystem
{
    public void CreateHardLink(string sourceFileName, string destFileName)
    {
        if (!CreateHardLinkNative(destFileName, sourceFileName, IntPtr.Zero))
            ThrowExceptionForLastError();
    }

    public void CreateFileSymbolicLink(string path, string pathToTarget) => CreateSymbolicLink(path, pathToTarget, false);

    public void CreateDirectorySymbolicLink(string path, string pathToTarget) => CreateSymbolicLink(path, pathToTarget, true);

    private static void CreateSymbolicLink(string path, string pathToTarget, bool isDirectory)
    {
        path = PathInternal.EnsureExtendedPrefixIfNeeded(path);
        pathToTarget = PathInternal.EnsureExtendedPrefixIfNeeded(pathToTarget);

        SymbolicLinkFlags flags = 0;
        if (isDirectory)
            flags |= SymbolicLinkFlags.SYMBOLIC_LINK_FLAG_DIRECTORY;
        if (Environment.OSVersion.Version is { Major: >= 11 } or { Major: 10, Build: >= 14972 })
            flags |= SymbolicLinkFlags.SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE;

        if (!CreateSymbolicLinkNative(path, pathToTarget, flags))
            ThrowExceptionForLastError();
    }

    private static void ThrowExceptionForLastError()
    {
        try
        {
            Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
        }
        catch (COMException exception)
        {
            throw new IOException(exception.Message);
        }
    }

    [DllImport("kernel32", EntryPoint = "CreateHardLinkW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateHardLinkNative(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

    [DllImport("kernel32", EntryPoint = "CreateSymbolicLinkW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateSymbolicLinkNative(string lpSymlinkFileName, string lpTargetFileName, SymbolicLinkFlags dwFlags);

    [Flags]
    private enum SymbolicLinkFlags : uint
    {
        // ReSharper disable InconsistentNaming
        SYMBOLIC_LINK_FLAG_DIRECTORY = 1 << 0,
        SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE = 1 << 1,
        // ReSharper restore InconsistentNaming
    }
}