using System;

namespace FileSystemLinks;

public static class FileSystemLink
{
    private static readonly IFileSystem FileSystem;

    static FileSystemLink()
    {
        FileSystem = Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => new WindowsFileSystem(),
            PlatformID.Unix or PlatformID.MacOSX => new UnixFileSystem(),
            _ => throw new PlatformNotSupportedException()
        };
    }

    public static void CreateHardLink(string sourceFileName, string destFileName)
    {
        if (sourceFileName is null)
            throw new ArgumentNullException(nameof(sourceFileName));
        if (string.IsNullOrWhiteSpace(sourceFileName))
            throw new ArgumentException("sourceFileName is empty.", nameof(sourceFileName));
        if (destFileName is null)
            throw new ArgumentNullException(nameof(destFileName));
        if (string.IsNullOrWhiteSpace(destFileName))
            throw new ArgumentException("destFileName is empty.", nameof(destFileName));

        FileSystem.CreateHardLink(sourceFileName, destFileName);
    }
}