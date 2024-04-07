using System.Runtime.InteropServices;
using System;

namespace FileSystemLinks;

internal class MonoPosixErrorResolver : IPosixErrorResolver
{
    public PosixError GetErrorFromNativeErrorCode(int errorCode)
    {
        if (ToErrnoNative(errorCode, out var monoError) != 0)
            return PosixError.UnknownError + errorCode;
        return monoError switch
        {
            MonoPosixError.EACCES => PosixError.EACCES,
            MonoPosixError.EBADF => PosixError.EBADF,
            MonoPosixError.EINVAL => PosixError.EINVAL,
            MonoPosixError.ELOOP => PosixError.ELOOP,
            MonoPosixError.ENAMETOOLONG => PosixError.ENAMETOOLONG,
            MonoPosixError.ENOENT => PosixError.ENOENT,
            MonoPosixError.ENOTDIR => PosixError.ENOTDIR,
            MonoPosixError.EPERM => PosixError.EPERM,
            _ => PosixError.UnknownError + errorCode,
        };
    }

    public int GetNativeErrorCodeFromError(PosixError error)
    {
        if (error >= PosixError.UnknownError)
            return error - PosixError.UnknownError;
        var monoError = error switch
        {
            PosixError.EACCES => MonoPosixError.EACCES,
            PosixError.EBADF => MonoPosixError.EBADF,
            PosixError.EINVAL => MonoPosixError.EINVAL,
            PosixError.ELOOP => MonoPosixError.ELOOP,
            PosixError.ENAMETOOLONG => MonoPosixError.ENAMETOOLONG,
            PosixError.ENOENT => MonoPosixError.ENOENT,
            PosixError.ENOTDIR => MonoPosixError.ENOTDIR,
            PosixError.EPERM => MonoPosixError.EPERM,
            _ => throw new ArgumentOutOfRangeException(nameof(error))
        };
        if (FromErrnoNative(monoError, out var errorCode) != 0)
            throw new ArgumentOutOfRangeException(nameof(error));
        return errorCode;
    }

    public string GetStringFromNativeErrorCode(int errorCode)
    {
        const int maxBufferLength = 1024; // should be long enough for most any UNIX error
        var buffer = Marshal.AllocHGlobal(maxBufferLength);
        try
        {
            // If function fails, the buffer should still contain some generic error message.
            StrErrorRNative(errorCode, buffer, (IntPtr)maxBufferLength);
            return Marshal.PtrToStringAnsi(buffer)!;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    // Values taken from https://github.com/mono/mono/blob/6be3e701766af6e3971e5ba260ece1a3ab095c08/mcs/class/Mono.Posix/Mono.Unix.Native/Stdlib.cs
    private enum MonoPosixError
    {
        // ReSharper disable InconsistentNaming
        EACCES = 13,
        EBADF = 9,
        EINVAL = 22,
        ELOOP = 40,
        ENAMETOOLONG = 36,
        ENOENT = 2,
        ENOTDIR = 20,
        EPERM = 1,
        // ReSharper restore InconsistentNaming
    }

    private const string MonoPosixHelperLibraryName = "libMonoPosixHelper";

    [DllImport(MonoPosixHelperLibraryName, EntryPoint = "Mono_Posix_FromErrno")]
    private static extern int FromErrnoNative(MonoPosixError value, out int rval);

    [DllImport(MonoPosixHelperLibraryName, EntryPoint = "Mono_Posix_ToErrno")]
    private static extern int ToErrnoNative(int value, out MonoPosixError rval);

    [DllImport(MonoPosixHelperLibraryName, EntryPoint = "Mono_Posix_Syscall_strerror_r", SetLastError = true)]
    private static extern int StrErrorRNative(int errnum, IntPtr buf, IntPtr n);
}