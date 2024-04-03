using System;
using System.IO;

namespace FileSystemLinks;

public static class FileSystemInfoExtensions
{
    public static void CreateHardLink(this FileInfo fileInfo, string destFileName)
    {
        if (fileInfo is null)
            throw new ArgumentNullException(nameof(fileInfo));

        FileSystemLink.CreateHardLink(fileInfo.FullName, destFileName);
    }

    public static void CreateAsSymbolicLink(this FileInfo fileInfo, string pathToTarget)
    {
        if (fileInfo is null)
            throw new ArgumentNullException(nameof(fileInfo));

        FileSystemLink.CreateFileSymbolicLink(fileInfo.FullName, pathToTarget);
    }

    public static void CreateAsSymbolicLink(this DirectoryInfo directoryInfo, string pathToTarget)
    {
        if (directoryInfo is null)
            throw new ArgumentNullException(nameof(directoryInfo));

        FileSystemLink.CreateDirectorySymbolicLink(directoryInfo.FullName, pathToTarget);
    }
}