namespace FileSystemLinks.Tests;

[TestClass]
public class GetDirectoryLinkTargetTests : TestBase
{
    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetExistsAndIsAbsolute_ReturnFirstTarget()
    {
        var targetDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var intermediateLinkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        Directory.CreateDirectory(targetDirectoryPath);
#if NET6_0_OR_GREATER
        File.CreateSymbolicLink(intermediateLinkDirectoryPath, targetDirectoryPath);
        File.CreateSymbolicLink(linkDirectoryPath, intermediateLinkDirectoryPath);
#else
        FileSystemLink.CreateFileSymbolicLink(intermediateLinkDirectoryPath, targetDirectoryPath);
        FileSystemLink.CreateFileSymbolicLink(linkDirectoryPath, intermediateLinkDirectoryPath);
#endif

        var returnedTarget = FileSystemLink.GetFileLinkTarget(linkDirectoryPath);

        Assert.AreEqual(intermediateLinkDirectoryPath, returnedTarget);
    }

    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetExistsAndIsRelative_ReturnFirstTarget()
    {
        var targetDirectoryPath = UnicodeString + Path.GetRandomFileName();
        var intermediateLinkDirectoryPath = UnicodeString + Path.GetRandomFileName();
        var linkDirectoryPath = UnicodeString + Path.GetRandomFileName();
        Directory.CreateDirectory(targetDirectoryPath);
#if NET6_0_OR_GREATER
        File.CreateSymbolicLink(intermediateLinkDirectoryPath, targetDirectoryPath);
        File.CreateSymbolicLink(linkDirectoryPath, intermediateLinkDirectoryPath);
#else
        FileSystemLink.CreateFileSymbolicLink(intermediateLinkDirectoryPath, targetDirectoryPath);
        FileSystemLink.CreateFileSymbolicLink(linkDirectoryPath, intermediateLinkDirectoryPath);
#endif

        var returnedTarget = FileSystemLink.GetFileLinkTarget(linkDirectoryPath);

        Assert.AreEqual(intermediateLinkDirectoryPath, returnedTarget);
    }

    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetDoesNotExistAndIsAbsolute_ReturnTarget()
    {
        var targetDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
#if NET6_0_OR_GREATER
        File.CreateSymbolicLink(linkDirectoryPath, targetDirectoryPath);
#else
        FileSystemLink.CreateFileSymbolicLink(linkDirectoryPath, targetDirectoryPath);
#endif

        var returnedTarget = FileSystemLink.GetFileLinkTarget(linkDirectoryPath);

        Assert.AreEqual(targetDirectoryPath, returnedTarget);
    }

    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetDoesNotExistAndIsRelative_ReturnTarget()
    {
        var targetDirectoryPath = UnicodeString + Path.GetRandomFileName();
        var linkDirectoryPath = UnicodeString + Path.GetRandomFileName();
#if NET6_0_OR_GREATER
        File.CreateSymbolicLink(linkDirectoryPath, targetDirectoryPath);
#else
        FileSystemLink.CreateFileSymbolicLink(linkDirectoryPath, targetDirectoryPath);
#endif

        var returnedTarget = FileSystemLink.GetFileLinkTarget(linkDirectoryPath);

        Assert.AreEqual(targetDirectoryPath, returnedTarget);
    }

    [TestMethod]
    public void WhenPathIsPlainFile_ReturnNull()
    {
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        File.WriteAllText(linkDirectoryPath, "test");

        var returnedTarget = FileSystemLink.GetFileLinkTarget(linkDirectoryPath);

        Assert.IsNull(returnedTarget);
    }

    [TestMethod]
    public void WhenPathIsPlainDirectory_ReturnNull()
    {
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        Directory.CreateDirectory(linkDirectoryPath);

        var returnedTarget = FileSystemLink.GetFileLinkTarget(linkDirectoryPath);

        Assert.IsNull(returnedTarget);
    }

    [TestMethod]
    public void WhenPathDoesNotExist_ReturnNull()
    {
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());

        var returnedTarget = FileSystemLink.GetFileLinkTarget(linkDirectoryPath);

        Assert.IsNull(returnedTarget);
    }
}