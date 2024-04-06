namespace FileSystemLinks;

internal interface IFileSystem
{
    void CreateHardLink(string sourceFileName, string destFileName);
    void CreateSymbolicLink(string path, string pathToTarget, bool isDirectory);
    void CreateJunction(string path, string pathToTarget);
    string? GetLinkTarget(string linkPath, bool isDirectory);
    string? ResolveLinkTarget(string linkPath, bool returnFinalTarget, bool isDirectory);
}