using System;
using System.IO;

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

    public static string? GetFileLinkTarget(string linkPath)
    {
        VerifyValidPath(linkPath, nameof(linkPath));

        return FileSystem.GetLinkTarget(linkPath, false);
    }

    public static string? GetDirectoryLinkTarget(string linkPath)
    {
        VerifyValidPath(linkPath, nameof(linkPath));

        return FileSystem.GetLinkTarget(linkPath, true);
    }

    public static FileSystemInfo? ResolveFileLinkTarget(string linkPath, bool returnFinalTarget) => 
        ResolveLinkTarget(linkPath, returnFinalTarget, false);

    public static FileSystemInfo? ResolveDirectoryLinkTarget(string linkPath, bool returnFinalTarget) => 
        ResolveLinkTarget(linkPath, returnFinalTarget, true);

    private static FileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget, bool isDirectory)
    {
        VerifyValidPath(linkPath, nameof(linkPath));

        var linkTarget = FileSystem.ResolveLinkTarget(linkPath, returnFinalTarget, isDirectory);
        return linkTarget is null ? null : isDirectory ? new DirectoryInfo(linkTarget) : new FileInfo(linkTarget);
    }

    private static void VerifyValidPath(string path, string paramName)
    {
        if (path is null)
            throw new ArgumentNullException(paramName);
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("The path is empty.", paramName);
        if (path.Contains("\0"))
            throw new ArgumentException("Illegal characters in path.", paramName);
    }
}