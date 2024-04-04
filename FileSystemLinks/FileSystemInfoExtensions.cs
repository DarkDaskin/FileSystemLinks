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

    public static void CreateAsSymbolicLink(this FileSystemInfo fileSystemInfo, string pathToTarget)
    {
        if (fileSystemInfo is null)
            throw new ArgumentNullException(nameof(fileSystemInfo));

        if (fileSystemInfo is DirectoryInfo)
            FileSystemLink.CreateDirectorySymbolicLink(fileSystemInfo.FullName, pathToTarget);
        else
            FileSystemLink.CreateFileSymbolicLink(fileSystemInfo.FullName, pathToTarget);
    }

    public static string? GetLinkTarget(this FileSystemInfo fileSystemInfo)
    {
        if (fileSystemInfo is null)
            throw new ArgumentNullException(nameof(fileSystemInfo));

        return fileSystemInfo is DirectoryInfo
            ? FileSystemLink.GetDirectoryLinkTarget(fileSystemInfo.FullName)
            : FileSystemLink.GetFileLinkTarget(fileSystemInfo.FullName);
    }
}