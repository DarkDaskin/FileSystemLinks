using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FileSystemLinks;

internal class UnixFileSystem : IFileSystem
{
    public void CreateHardLink(string sourceFileName, string destFileName)
    {
        if (CreateHardLinkNative(sourceFileName, destFileName) != 0)
            ThrowExceptionForLastError(false);
    }

    private static void ThrowExceptionForLastError(bool isDirectory)
    {
        var errorCode = Marshal.GetLastWin32Error();
        var messagePtr = GetErrorMessageNative(errorCode);
        var message = Marshal.PtrToStringAnsi(messagePtr);
        switch ((ErrorCode)errorCode)
        {
            case ErrorCode.ENOENT when isDirectory:
                throw new DirectoryNotFoundException(message);
            case ErrorCode.ENOENT:
                throw new FileNotFoundException(message);
            case ErrorCode.EACCES:
                throw new UnauthorizedAccessException(message);
            default:
                throw new IOException(message);
        }
    }

    [DllImport("libc", EntryPoint = "link", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int CreateHardLinkNative(string oldpath, string newpath);

    [DllImport("libc", EntryPoint = "strerror")]
    private static extern IntPtr GetErrorMessageNative(int errnum);

    private enum ErrorCode
    {
        // ReSharper disable InconsistentNaming
        ENOENT = 2,
        EACCES = 13,
        // ReSharper restore InconsistentNaming
    }
}