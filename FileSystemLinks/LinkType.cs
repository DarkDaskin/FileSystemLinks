namespace FileSystemLinks;

/// <summary>
/// Represents the type of file system link. 
/// </summary>
/// <remarks>
/// Hard links are just additional file names for a file rather than a special entity in a file system
/// and thus are not recognized as a type of link.
/// </remarks>
public enum LinkType
{
    /// <summary>
    /// The file or directory is not a link.
    /// </summary>
    None,
    /// <summary>
    /// The file or directory is a symbolic link.
    /// </summary>
    SymbolicLink,
    /// <summary>
    /// The directory is a junction.
    /// </summary>
    Junction,
}