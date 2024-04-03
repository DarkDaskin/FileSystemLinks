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
}