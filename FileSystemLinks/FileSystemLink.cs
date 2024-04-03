using System;

namespace FileSystemLinks;

public static class FileSystemLink
{
    private const string EmptyFileNameExceptionMessage = "Empty file name is not legal.";
    private const string EmptyPathExceptionMessage = "Path cannot be the empty string or all whitespace.";

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
            throw new ArgumentException(EmptyFileNameExceptionMessage, nameof(sourceFileName));
        if (destFileName is null)
            throw new ArgumentNullException(nameof(destFileName));
        if (string.IsNullOrWhiteSpace(destFileName))
            throw new ArgumentException(EmptyFileNameExceptionMessage, nameof(destFileName));

        FileSystem.CreateHardLink(sourceFileName, destFileName);
    }

    public static void CreateFileSymbolicLink(string path, string pathToTarget)
    {
        if (path is null)
            throw new ArgumentNullException(nameof(path));
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException(EmptyPathExceptionMessage, nameof(path));
        if (pathToTarget is null)
            throw new ArgumentNullException(nameof(pathToTarget));
        if (string.IsNullOrWhiteSpace(pathToTarget))
            throw new ArgumentException(EmptyPathExceptionMessage, nameof(pathToTarget));

        FileSystem.CreateFileSymbolicLink(path, pathToTarget);
    }

    public static void CreateDirectorySymbolicLink(string path, string pathToTarget)
    {
        if (path is null)
            throw new ArgumentNullException(nameof(path));
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException(EmptyPathExceptionMessage, nameof(path));
        if (pathToTarget is null)
            throw new ArgumentNullException(nameof(pathToTarget));
        if (string.IsNullOrWhiteSpace(pathToTarget))
            throw new ArgumentException(EmptyPathExceptionMessage, nameof(pathToTarget));

        FileSystem.CreateDirectorySymbolicLink(path, pathToTarget);
    }
}