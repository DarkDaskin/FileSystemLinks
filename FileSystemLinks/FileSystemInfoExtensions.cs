using System;
using System.IO;
using System.Runtime.Versioning;

namespace FileSystemLinks;

/// <summary>
/// Contains extension methods for <see cref="FileSystemInfo"/> instances.
/// </summary>
public static class FileSystemInfoExtensions
{
    /// <summary>
    /// Establishes a hard link between an existing file and a new file.
    /// </summary>
    /// <param name="fileInfo">A <see cref="FileInfo"/> instance representing an existing file.</param>
    /// <param name="destFileName">The name of the new file.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="fileInfo"/> or <paramref name="destFileName"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="destFileName"/> is an empty string, all whitespace or contains invalid characters.
    /// </exception> 
    /// <exception cref="PathTooLongException">
    /// The specified path, file name, or both exceed the system-defined maximum length.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// <paramref name="fileInfo"/> was not found.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller does not have the required permission.
    /// </exception>
    /// <exception cref="IOException">
    /// <paramref name="destFileName"/> already exists.
    /// - or -
    /// <paramref name="fileInfo"/> and <paramref name="destFileName"/> are on different file systems.
    /// </exception>
    public static void CreateHardLink(this FileInfo fileInfo, string destFileName)
    {
        if (fileInfo is null)
            throw new ArgumentNullException(nameof(fileInfo));

        FileSystemLink.CreateHardLink(fileInfo.FullName, destFileName);
    }

    /// <summary>
    /// Creates a symbolic link located in <see cref="FileSystemInfo.FullName"/> that points to the specified <paramref name="pathToTarget"/>.
    /// </summary>
    /// <param name="fileSystemInfo">A <see cref="FileSystemInfo"/> instance representing a symbolic link to be created.</param>
    /// <param name="pathToTarget">The path of the target to which the symbolic link points.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="fileSystemInfo"/> or <paramref name="pathToTarget"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="pathToTarget"/> is an empty string, all whitespace or contains invalid characters.
    /// </exception> 
    /// <exception cref="PathTooLongException">
    /// The specified path, file name, or both exceed the system-defined maximum length.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller does not have the required permission.
    /// </exception>
    /// <exception cref="IOException">
    /// The file or directory at <paramref name="pathToTarget"/> already exists.
    /// -or-
    /// An I/O error occurred.
    /// </exception>
    public static void CreateAsSymbolicLink(this FileSystemInfo fileSystemInfo, string pathToTarget)
    {
        if (fileSystemInfo is null)
            throw new ArgumentNullException(nameof(fileSystemInfo));

        if (fileSystemInfo is DirectoryInfo)
            FileSystemLink.CreateDirectorySymbolicLink(fileSystemInfo.FullName, pathToTarget);
        else
            FileSystemLink.CreateFileSymbolicLink(fileSystemInfo.FullName, pathToTarget);
    }

    /// <summary>
    /// Creates a junction located in <see cref="DirectoryInfo.FullName"/> that points to the specified <paramref name="pathToTarget"/>.
    /// </summary>
    /// <param name="directoryInfo">A <see cref="DirectoryInfo"/> instance representing a junction to be created.</param>
    /// <param name="pathToTarget">The path of the target to which the junction points.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="directoryInfo"/> or <paramref name="pathToTarget"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="pathToTarget"/> is an empty string, all whitespace or contains invalid characters.
    /// </exception> 
    /// <exception cref="PathTooLongException">
    /// The specified path, file name, or both exceed the system-defined maximum length.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller does not have the required permission.
    /// </exception>
    /// <exception cref="IOException">
    /// The file or directory at <paramref name="pathToTarget"/> already exists.
    /// -or-
    /// <paramref name="pathToTarget"/> points to a network location.
    /// -or-
    /// An I/O error occurred.
    /// </exception>
    /// <exception cref="PlatformNotSupportedException">
    /// This method is called on a platform different from Windows.
    /// </exception>
#if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public static void CreateAsJunction(this DirectoryInfo directoryInfo, string pathToTarget)
    {
        if (directoryInfo is null)
            throw new ArgumentNullException(nameof(directoryInfo));

        FileSystemLink.CreateJunction(directoryInfo.FullName, pathToTarget);
    }

    /// <summary>
    /// Gets the target path of the link represented by <paramref name="fileSystemInfo"/>, or <see langword="null"/> if <paramref name="fileSystemInfo"/> doesn't represent a link.
    /// </summary>
    /// <param name="fileSystemInfo">A <see cref="FileSystemInfo"/> instance representing the link.</param>
    /// <returns>
    /// The target path of the file link located at <paramref name="fileSystemInfo"/>, or <see langword="null"/> if <paramref name="fileSystemInfo"/> doesn't represent a link.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="fileSystemInfo"/> is <see langword="null"/>.
    /// </exception>
    public static string? GetLinkTarget(this FileSystemInfo fileSystemInfo)
    {
        if (fileSystemInfo is null)
            throw new ArgumentNullException(nameof(fileSystemInfo));

        return fileSystemInfo is DirectoryInfo
            ? FileSystemLink.GetDirectoryLinkTarget(fileSystemInfo.FullName)
            : FileSystemLink.GetFileLinkTarget(fileSystemInfo.FullName);
    }

    /// <summary>
    /// Gets the target of the link represented by <paramref name="fileSystemInfo"/>, or <see langword="null"/> if <paramref name="fileSystemInfo"/> is not a link.
    /// </summary>
    /// <param name="fileSystemInfo">A <see cref="FileSystemInfo"/> instance representing the link.</param>
    /// <param name="returnFinalTarget"><see langword="true"/> to follow links to the final target; <see langword="false"/> to return the immediate next link.</param>
    /// <returns>A <see cref="FileSystemInfo"/> instance if <paramref name="fileSystemInfo"/> exists, independently if the target exists or not. <see langword="null"/> if <paramref name="fileSystemInfo"/> is not a link.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="fileSystemInfo"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller does not have the required permission.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// The <paramref name="fileSystemInfo"/> does not exist.
    /// </exception>
    /// <exception cref="IOException">
    /// An I/O error occurred.
    /// </exception>
    public static FileSystemInfo? ResolveLinkTarget(this FileSystemInfo fileSystemInfo, bool returnFinalTarget)
    {
        if (fileSystemInfo is null)
            throw new ArgumentNullException(nameof(fileSystemInfo));

        return fileSystemInfo is DirectoryInfo
            ? FileSystemLink.ResolveDirectoryLinkTarget(fileSystemInfo.FullName, returnFinalTarget)
            : FileSystemLink.ResolveFileLinkTarget(fileSystemInfo.FullName, returnFinalTarget);
    }
}