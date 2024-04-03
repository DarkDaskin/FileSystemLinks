using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FileSystemLinks;

internal class WindowsFileSystem : IFileSystem
{
    public void CreateHardLink(string sourceFileName, string destFileName)
    {
        if (!CreateHardLinkNative(destFileName, sourceFileName, IntPtr.Zero))
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
}