using System;
using System.Runtime.InteropServices;

namespace FileSystemLinks;

internal class NetCorePosixErrorResolver : IPosixErrorResolver
{
    public PosixError GetErrorFromNativeErrorCode(int errorCode) => ConvertErrorPlatformToPalNative(errorCode);

    public int GetNativeErrorCodeFromError(PosixError error) => ConvertErrorPalToPlatformNative(error);

    public string GetStringFromNativeErrorCode(int errorCode)
    {
        const int maxBufferLength = 1024; // should be long enough for most any UNIX error
        var buffer = Marshal.AllocHGlobal(maxBufferLength);
        try
        {
            var message = StrErrorRNative(errorCode, buffer, maxBufferLength);

            if (message == IntPtr.Zero)
            {
                // This means the buffer was not large enough, but still contains
                // as much of the error message as possible and is guaranteed to
                // be null-terminated. We're not currently resizing/retrying because
                // MaxBufferLength is large enough in practice, but we could do
                // so here in the future if necessary.
                message = buffer;
            }

            return Marshal.PtrToStringAnsi(message)!;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private const string SystemNativeLibraryName = "libSystem.Native";

    [DllImport(SystemNativeLibraryName, EntryPoint = "SystemNative_ConvertErrorPlatformToPal")]
    internal static extern PosixError ConvertErrorPlatformToPalNative(int platformErrno);

    [DllImport(SystemNativeLibraryName, EntryPoint = "SystemNative_ConvertErrorPalToPlatform")]
    internal static extern int ConvertErrorPalToPlatformNative(PosixError error);

    [DllImport(SystemNativeLibraryName, EntryPoint = "SystemNative_StrErrorR")]
    private static extern IntPtr StrErrorRNative(int platformErrno, IntPtr buffer, int bufferSize);
}