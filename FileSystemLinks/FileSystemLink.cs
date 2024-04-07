using System;
using System.IO;
using System.Runtime.Versioning;

namespace FileSystemLinks;

/// <summary>
/// Provides static methods for creating and reading file system links.
/// </summary>
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

    /// <summary>
    /// Establishes a hard link between an existing file and a new file.
    /// </summary>
    /// <param name="sourceFileName">The name of the existing file.</param>
    /// <param name="destFileName">The name of the new file.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sourceFileName"/> or <paramref name="destFileName"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="sourceFileName"/> or <paramref name="destFileName"/> is an empty string, all whitespace or contains invalid characters.
    /// </exception> 
    /// <exception cref="PathTooLongException">
    /// The specified path, file name, or both exceed the system-defined maximum length.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// <paramref name="sourceFileName"/> was not found.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller does not have the required permission.
    /// </exception>
    /// <exception cref="IOException">
    /// <paramref name="destFileName"/> already exists.
    /// -or-
    /// <paramref name="sourceFileName"/> and <paramref name="destFileName"/> are on different file systems.
    /// -or-
    /// An I/O error occurred.
    /// </exception>
    public static void CreateHardLink(string sourceFileName, string destFileName)
    {
        VerifyValidPath(sourceFileName, nameof(sourceFileName));
        VerifyValidPath(destFileName, nameof(destFileName));

        FileSystem.CreateHardLink(sourceFileName, destFileName);
    }

    /// <summary>
    /// Creates a file symbolic link identified by <paramref name="path"/> that points to <paramref name="pathToTarget"/>.
    /// </summary>
    /// <param name="path">The path where the symbolic link should be created.</param>
    /// <param name="pathToTarget">The path of the target to which the symbolic link points.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path"/> or <paramref name="pathToTarget"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> or <paramref name="pathToTarget"/> is an empty string, all whitespace or contains invalid characters.
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
    public static void CreateFileSymbolicLink(string path, string pathToTarget)
    {
        VerifyValidPath(path, nameof(path));
        VerifyValidPath(pathToTarget, nameof(pathToTarget));

        FileSystem.CreateSymbolicLink(path, pathToTarget, false);
    }

    /// <summary>
    /// Creates a directory symbolic link identified by path that points to pathToTarget.
    /// </summary>
    /// <param name="path">The path where the symbolic link should be created.</param>
    /// <param name="pathToTarget">The path of the target to which the symbolic link points.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path"/> or <paramref name="pathToTarget"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> or <paramref name="pathToTarget"/> is an empty string, all whitespace or contains invalid characters.
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
    public static void CreateDirectorySymbolicLink(string path, string pathToTarget)
    {
        VerifyValidPath(path, nameof(path));
        VerifyValidPath(pathToTarget, nameof(pathToTarget));

        FileSystem.CreateSymbolicLink(path, pathToTarget, true);
    }

    /// <summary>
    /// Creates a junction identified by path that points to pathToTarget.
    /// </summary>
    /// <param name="path">The path where the junction should be created.</param>
    /// <param name="pathToTarget">The path of the target to which the junction points.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path"/> or <paramref name="pathToTarget"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> or <paramref name="pathToTarget"/> is an empty string, all whitespace or contains invalid characters.
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
    public static void CreateJunction(string path, string pathToTarget)
    {
        VerifyValidPath(path, nameof(path));
        VerifyValidPath(pathToTarget, nameof(pathToTarget));

        FileSystem.CreateJunction(path, pathToTarget);
    }

    /// <summary>
    /// Gets the target path of the file link located at <paramref name="linkPath"/>, or <see langword="null"/> if <paramref name="linkPath"/> doesn't represent a link.
    /// </summary>
    /// <param name="linkPath">The path of the file link.</param>
    /// <returns>
    /// The target path of the file link located at <paramref name="linkPath"/>, or <see langword="null"/> if <paramref name="linkPath"/> doesn't represent a link.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="linkPath"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="linkPath"/> is an empty string, all whitespace or contains invalid characters.
    /// </exception> 
    public static string? GetFileLinkTarget(string linkPath)
    {
        VerifyValidPath(linkPath, nameof(linkPath));

        return FileSystem.GetLinkTarget(linkPath, false);
    }

    /// <summary>
    /// Gets the target path of the directory link located at <paramref name="linkPath"/>, or <see langword="null"/> if <paramref name="linkPath"/> doesn't represent a link.
    /// </summary>
    /// <param name="linkPath">The path of the directory link.</param>
    /// <returns>
    /// The target path of the directory link located at <paramref name="linkPath"/>, or <see langword="null"/> if <paramref name="linkPath"/> doesn't represent a link.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="linkPath"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="linkPath"/> is an empty string, all whitespace or contains invalid characters.
    /// </exception> 
    public static string? GetDirectoryLinkTarget(string linkPath)
    {
        VerifyValidPath(linkPath, nameof(linkPath));

        return FileSystem.GetLinkTarget(linkPath, true);
    }

    /// <summary>
    /// Gets the target of the file link located at <paramref name="linkPath"/>, or <see langword="null"/> if <paramref name="linkPath"/> is not a link.
    /// </summary>
    /// <param name="linkPath">The path of the file link.</param>
    /// <param name="returnFinalTarget"><see langword="true"/> to follow links to the final target; <see langword="false"/> to return the immediate next link.</param>
    /// <returns>
    /// A <see cref="FileInfo"/> instance if <paramref name="linkPath"/> exists, independently if the target exists or not. <see langword="null"/> if <paramref name="linkPath"/> is not a link.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="linkPath"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="linkPath"/> is an empty string, all whitespace or contains invalid characters.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller does not have the required permission.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// The <paramref name="linkPath"/> does not exist.
    /// </exception>
    /// <exception cref="IOException">
    /// An I/O error occurred.
    /// </exception>
    public static FileSystemInfo? ResolveFileLinkTarget(string linkPath, bool returnFinalTarget) => 
        ResolveLinkTarget(linkPath, returnFinalTarget, false);

    /// <summary>
    /// Gets the target of the directory link located at <paramref name="linkPath"/>, or <see langword="null"/> if <paramref name="linkPath"/> is not a link.
    /// </summary>
    /// <param name="linkPath">The path of the directory link.</param>
    /// <param name="returnFinalTarget"><see langword="true"/> to follow links to the final target; <see langword="false"/> to return the immediate next link.</param>
    /// <returns>
    /// A <see cref="DirectoryInfo"/> instance if <paramref name="linkPath"/> exists, independently if the target exists or not. <see langword="null"/> if <paramref name="linkPath"/> is not a link.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="linkPath"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="linkPath"/> is an empty string, all whitespace or contains invalid characters.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller does not have the required permission.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// The <paramref name="linkPath"/> does not exist.
    /// </exception>
    /// <exception cref="IOException">
    /// An I/O error occurred.
    /// </exception>
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