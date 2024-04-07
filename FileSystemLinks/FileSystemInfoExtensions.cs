using System;
using System.IO;
using System.Runtime.Versioning;

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

#if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public static void CreateAsJunction(this DirectoryInfo directoryInfo, string pathToTarget)
    {
        if (directoryInfo is null)
            throw new ArgumentNullException(nameof(directoryInfo));

        FileSystemLink.CreateJunction(directoryInfo.FullName, pathToTarget);
    }

    public static string? GetLinkTarget(this FileSystemInfo fileSystemInfo)
    {
        if (fileSystemInfo is null)
            throw new ArgumentNullException(nameof(fileSystemInfo));

        return fileSystemInfo is DirectoryInfo
            ? FileSystemLink.GetDirectoryLinkTarget(fileSystemInfo.FullName)
            : FileSystemLink.GetFileLinkTarget(fileSystemInfo.FullName);
    }

    public static FileSystemInfo? ResolveLinkTarget(this FileSystemInfo fileSystemInfo, bool returnFinalTarget)
    {
        if (fileSystemInfo is null)
            throw new ArgumentNullException(nameof(fileSystemInfo));

        return fileSystemInfo is DirectoryInfo
            ? FileSystemLink.ResolveDirectoryLinkTarget(fileSystemInfo.FullName, returnFinalTarget)
            : FileSystemLink.ResolveFileLinkTarget(fileSystemInfo.FullName, returnFinalTarget);
    }
}