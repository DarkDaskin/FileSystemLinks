using System;
#if NETSTANDARD2_1_OR_GREATER
using System.Buffers;
#endif
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FileSystemLinks;

internal class UnixFileSystem : IFileSystem
{
    public void CreateHardLink(string sourceFileName, string destFileName)
    {
        if (CreateHardLinkNative(sourceFileName, destFileName) < 0)
            ThrowExceptionForLastError(false);
    }

    public void CreateSymbolicLink(string path, string pathToTarget, bool isDirectory)
    {
        if (CreateSymbolicLinkNative(pathToTarget, path) < 0)
            ThrowExceptionForLastError(isDirectory);
    }

    public string? GetLinkTarget(string linkPath, bool isDirectory) => ReadLink(linkPath);

    private static string? ReadLink(string linkPath)
    {
#if NETSTANDARD2_1_OR_GREATER
        byte[]? arrayBuffer = null;
        Span<byte> spanBuffer = stackalloc byte[InitialReadLinkBufferSize];
        while (true)
        {
            try
            {
                var resultLength = ReadLinkNative(linkPath, ref MemoryMarshal.GetReference(spanBuffer), (IntPtr)spanBuffer.Length);
            
                if (resultLength < 0)
                    return null;
                if (resultLength < spanBuffer.Length)
                    return Encoding.UTF8.GetString(spanBuffer[..resultLength]);
            }
            finally
            {
                if (arrayBuffer is not null)
                    ArrayPool<byte>.Shared.Return(arrayBuffer);
            }

            // Output buffer was too small, loop around again and try with a larger buffer.
            arrayBuffer = ArrayPool<byte>.Shared.Rent(spanBuffer.Length * 2);
            spanBuffer = arrayBuffer;
        }
#else
        var buffer = new byte[InitialReadLinkBufferSize];
        while (true)
        {
            var resultLength = ReadLinkNative(linkPath, buffer, (IntPtr)buffer.Length);
            
            if (resultLength < 0)
                return null;
            if (resultLength < buffer.Length)
                return Encoding.UTF8.GetString(buffer, 0, resultLength);

            // Output buffer was too small, loop around again and try with a larger buffer.
            buffer = new byte[buffer.Length * 2];
        }
#endif
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
            case ErrorCode.ENAMETOOLONG:
                throw new PathTooLongException(message);
            default:
                throw new IOException(message);
        }
    }

    [DllImport("libc", EntryPoint = "link", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int CreateHardLinkNative(string oldpath, string newpath);

    [DllImport("libc", EntryPoint = "symlink", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int CreateSymbolicLinkNative(string target, string linkpath);

    [DllImport("libc", EntryPoint = "readlink", CharSet = CharSet.Ansi, SetLastError = true)]
#if NETSTANDARD2_1_OR_GREATER
    private static extern int ReadLinkNative(string pathname, ref byte buf, IntPtr bufsiz);
#else
    private static extern int ReadLinkNative(string pathname, [Out] byte[] buf, IntPtr bufsiz);
#endif

    [DllImport("libc", EntryPoint = "strerror")]
    private static extern IntPtr GetErrorMessageNative(int errnum);

    private enum ErrorCode
    {
        // ReSharper disable InconsistentNaming
        ENOENT = 2,
        EACCES = 13,
        ENAMETOOLONG = 36,
        // ReSharper restore InconsistentNaming
    }

    private const int InitialReadLinkBufferSize = 256;
}