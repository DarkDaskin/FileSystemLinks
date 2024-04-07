namespace FileSystemLinks.Tests;

[TestClass]
public class GetLinkTypeTests : TestBase
{
    [TestMethod]
    public void WhenSymbolicLinkToFileIsSpecified_ReturnSymbolicLink()
    {
        var targetPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
#if NET6_0_OR_GREATER
        File.CreateSymbolicLink(linkPath, targetPath);
#else
        FileSystemLink.CreateFileSymbolicLink(linkPath, targetPath);
#endif

        var type = FileSystemLink.GetLinkType(linkPath);

        Assert.AreEqual(LinkType.SymbolicLink, type);
    }

    [TestMethod]
    public void WhenSymbolicLinkToDirectoryIsSpecified_ReturnSymbolicLink()
    {
        var targetPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
#if NET6_0_OR_GREATER
        Directory.CreateSymbolicLink(linkPath, targetPath);
#else
        FileSystemLink.CreateDirectorySymbolicLink(linkPath, targetPath);
#endif

        var type = FileSystemLink.GetLinkType(linkPath);

        Assert.AreEqual(LinkType.SymbolicLink, type);
    }

    [TestMethod]
    public void WhenJunctionIsSpecified_ReturnJunction()
    {
        if (!IsWindows())
        {
            Assert.Inconclusive();
            return;
        }

        var targetPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        FileSystemLink.CreateJunction(linkPath, targetPath);

        var type = FileSystemLink.GetLinkType(linkPath);

        Assert.AreEqual(LinkType.Junction, type);
    }

    [TestMethod]
    public void WhenPlainFileIsSpecified_ReturnNone()
    {
        var linkPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        File.WriteAllText(linkPath, "test");

        var type = FileSystemLink.GetLinkType(linkPath);

        Assert.AreEqual(LinkType.None, type);
    }

    [TestMethod]
    public void WhenPlainDirectoryIsSpecified_ReturnNone()
    {
        var linkPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        Directory.CreateDirectory(linkPath);

        var type = FileSystemLink.GetLinkType(linkPath);

        Assert.AreEqual(LinkType.None, type);
    }

    [TestMethod]
    public void WhenNonExistingPathIsSpecified_ReturnNone()
    {
        var linkPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        
        Assert.IsFalse(File.Exists(linkPath) || Directory.Exists(linkPath));

        var type = FileSystemLink.GetLinkType(linkPath);

        Assert.AreEqual(LinkType.None, type);
    }
}