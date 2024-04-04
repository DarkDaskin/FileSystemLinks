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
        VerifyValidPath(sourceFileName, nameof(sourceFileName));
        VerifyValidPath(destFileName, nameof(destFileName));

        FileSystem.CreateHardLink(sourceFileName, destFileName);
    }

    public static void CreateFileSymbolicLink(string path, string pathToTarget)
    {
        VerifyValidPath(path, nameof(path));
        VerifyValidPath(pathToTarget, nameof(pathToTarget));

        FileSystem.CreateSymbolicLink(path, pathToTarget, false);
    }

    public static void CreateDirectorySymbolicLink(string path, string pathToTarget)
    {
        VerifyValidPath(path, nameof(path));
        VerifyValidPath(pathToTarget, nameof(pathToTarget));

        FileSystem.CreateSymbolicLink(path, pathToTarget, true);
    }

    public static string? GetFileLinkTarget(string path)
    {
        VerifyValidPath(path, nameof(path));

        return FileSystem.GetLinkTarget(path, false);
    }

    public static string? GetDirectoryLinkTarget(string path)
    {
        VerifyValidPath(path, nameof(path));

        return FileSystem.GetLinkTarget(path, true);
    }

    private static void VerifyValidPath(string path, string argName)
    {
        if (path is null)
            throw new ArgumentNullException(nameof(path));
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("The path is empty.", nameof(path));
        if (path.Contains("\0"))
            throw new ArgumentException("Illegal characters in path.", argName);
    }
}