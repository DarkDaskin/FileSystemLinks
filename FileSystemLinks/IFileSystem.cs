namespace FileSystemLinks;

internal interface IFileSystem
{
    void CreateHardLink(string sourceFileName, string destFileName);
}