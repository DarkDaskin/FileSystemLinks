namespace FileSystemLinks;

internal interface IFileSystem
{
    void CreateHardLink(string sourceFileName, string destFileName);
    void CreateFileSymbolicLink(string path, string pathToTarget);
    void CreateDirectorySymbolicLink(string path, string pathToTarget);
}