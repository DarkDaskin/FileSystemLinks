using System;
#if NETSTANDARD2_1_OR_GREATER
using System.Buffers;
#endif
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FileSystemLinks;

internal partial class UnixFileSystem : IFileSystem
{
    private static readonly IPosixErrorResolver PosixErrorResolver =
        Type.GetType("Mono.Runtime") is not null 
            ? new MonoPosixErrorResolver() 
            : new NetCorePosixErrorResolver();

    public void CreateHardLink(string sourceFileName, string destFileName)
    {
        if (CreateHardLinkNative(sourceFileName, destFileName) < 0)
            throw GetExceptionForLastError(false);
    }

    public void CreateSymbolicLink(string path, string pathToTarget, bool isDirectory)
    {
        if (CreateSymbolicLinkNative(pathToTarget, path) < 0)
            throw GetExceptionForLastError(isDirectory);
    }

    public void CreateJunction(string path, string pathToTarget) => throw new PlatformNotSupportedException();

    public string? GetLinkTarget(string linkPath, bool isDirectory) => ReadLink(linkPath);

    public string? ResolveLinkTarget(string linkPath, bool returnFinalTarget, bool isDirectory)
    {
        linkPath = Path.GetFullPath(linkPath);

        var sb = new StringBuilder(DefaultPathBufferSize);
        sb.Append(linkPath);

        string? linkTarget = ReadLink(linkPath);
        if (linkTarget == null)
        {
            var errorCode = Marshal.GetLastWin32Error();
            // Not a link, return null
            if (PosixErrorResolver.GetErrorFromNativeErrorCode(errorCode) == PosixError.EINVAL)
                return null;

            throw GetExceptionForError(errorCode, isDirectory);
        }

        if (!returnFinalTarget)
        {
            GetLinkTargetFullPath(sb, linkTarget);
        }
        else
        {
            string? current = linkTarget;
            int visitCount = 1;

            while (current != null)
            {
                if (visitCount > MaxFollowedLinks)
                {
                    // We went over the limit and couldn't reach the final target
                    throw GetExceptionForError(PosixErrorResolver.GetNativeErrorCodeFromError(PosixError.ELOOP), isDirectory);
                }

                GetLinkTargetFullPath(sb, current);
                current = ReadLink(sb.ToString());
                visitCount++;
            }
        }

        Debug.Assert(sb.Length > 0);
        return sb.ToString();
    }

    private static string? ReadLink(string linkPath)
    {
#if NETSTANDARD2_1_OR_GREATER
        byte[]? arrayBuffer = null;
        Span<byte> spanBuffer = stackalloc byte[DefaultPathBufferSize];
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
        var buffer = new byte[DefaultPathBufferSize];
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

    // In case of link target being relative:
    // Preserve the full path of the directory of the previous path
    // so the final target is returned with a valid full path
    private static void GetLinkTargetFullPath(StringBuilder sb, string linkTarget)
    {
        if (PathInternal.IsPartiallyQualified(linkTarget))
        {
            sb.Length = PathInternal.GetDirectoryNameOffset(sb.ToString());
            sb.Append(PathInternal.DirectorySeparatorChar);
        }
        else
        {
            sb.Length = 0;
        }
        sb.Append(linkTarget);
    }

    private static Exception GetExceptionForLastError(bool isDirectory) => GetExceptionForError(Marshal.GetLastWin32Error(), isDirectory);

    private static Exception GetExceptionForError(int errorCode, bool isDirectory)
    {
        var message = PosixErrorResolver.GetStringFromNativeErrorCode(errorCode);
        var error = PosixErrorResolver.GetErrorFromNativeErrorCode(errorCode);
        return error switch
        {
            PosixError.ENOTDIR => new DirectoryNotFoundException(message),
            PosixError.ENOENT when isDirectory => new DirectoryNotFoundException(message),
            PosixError.ENOENT => new FileNotFoundException(message),
            PosixError.EACCES or PosixError.EBADF or PosixError.EPERM => new UnauthorizedAccessException(message),
            PosixError.ENAMETOOLONG => new PathTooLongException(message),
            _ => new IOException(message)
        };
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

    // Unix max paths are typically 1K or 4K UTF-8 bytes, 256 should handle the majority of paths
    // without putting too much pressure on the stack.
    internal const int DefaultPathBufferSize = 256;

    // On Linux, the maximum number of symbolic links that are followed while resolving a pathname is 40.
    // See: https://man7.org/linux/man-pages/man7/path_resolution.7.html
    private const int MaxFollowedLinks = 40;
}