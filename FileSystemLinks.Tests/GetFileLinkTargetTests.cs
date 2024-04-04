namespace FileSystemLinks.Tests;

[TestClass]
public class GetFileLinkTargetTests : TestBase
{
    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetExistsAndIsAbsolute_ReturnFirstTarget()
    {
        var targetFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var intermediateLinkFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        File.WriteAllText(targetFileName, "test");
#if NET6_0_OR_GREATER
        File.CreateSymbolicLink(intermediateLinkFileName, targetFileName);
        File.CreateSymbolicLink(linkFileName, intermediateLinkFileName);
#else
        FileSystemLink.CreateFileSymbolicLink(intermediateLinkFileName, targetFileName);
        FileSystemLink.CreateFileSymbolicLink(linkFileName, intermediateLinkFileName);
#endif

        var returnedTarget = FileSystemLink.GetFileLinkTarget(linkFileName);

        Assert.AreEqual(intermediateLinkFileName, returnedTarget);
    }

    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetExistsAndIsRelative_ReturnFirstTarget()
    {
        var targetFileName = UnicodeString + Path.GetRandomFileName();
        var intermediateLinkFileName = UnicodeString + Path.GetRandomFileName();
        var linkFileName = UnicodeString + Path.GetRandomFileName();
        File.WriteAllText(targetFileName, "test");
#if NET6_0_OR_GREATER
        File.CreateSymbolicLink(intermediateLinkFileName, targetFileName);
        File.CreateSymbolicLink(linkFileName, intermediateLinkFileName);
#else
        FileSystemLink.CreateFileSymbolicLink(intermediateLinkFileName, targetFileName);
        FileSystemLink.CreateFileSymbolicLink(linkFileName, intermediateLinkFileName);
#endif

        var returnedTarget = FileSystemLink.GetFileLinkTarget(linkFileName);

        Assert.AreEqual(intermediateLinkFileName, returnedTarget);
    }

    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetDoesNotExistAndIsAbsolute_ReturnTarget()
    {
        var targetFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
#if NET6_0_OR_GREATER
        File.CreateSymbolicLink(linkFileName, targetFileName);
#else
        FileSystemLink.CreateFileSymbolicLink(linkFileName, targetFileName);
#endif

        var returnedTarget = FileSystemLink.GetFileLinkTarget(linkFileName);

        Assert.AreEqual(targetFileName, returnedTarget);
    }

    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetDoesNotExistAndIsRelative_ReturnTarget()
    {
        var targetFileName = UnicodeString + Path.GetRandomFileName();
        var linkFileName = UnicodeString + Path.GetRandomFileName();
#if NET6_0_OR_GREATER
        File.CreateSymbolicLink(linkFileName, targetFileName);
#else
        FileSystemLink.CreateFileSymbolicLink(linkFileName, targetFileName);
#endif

        var returnedTarget = FileSystemLink.GetFileLinkTarget(linkFileName);

        Assert.AreEqual(targetFileName, returnedTarget);
    }

    [TestMethod]
    public void WhenPathIsPlainFile_ReturnNull()
    {
        var linkFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        File.WriteAllText(linkFileName, "test");

        var returnedTarget = FileSystemLink.GetFileLinkTarget(linkFileName);

        Assert.IsNull(returnedTarget);
    }

    [TestMethod]
    public void WhenPathIsPlainDirectory_ReturnNull()
    {
        var linkFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        Directory.CreateDirectory(linkFileName);

        var returnedTarget = FileSystemLink.GetFileLinkTarget(linkFileName);

        Assert.IsNull(returnedTarget);
    }

    [TestMethod]
    public void WhenPathDoesNotExist_ReturnNull()
    {
        var linkFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());

        var returnedTarget = FileSystemLink.GetFileLinkTarget(linkFileName);

        Assert.IsNull(returnedTarget);
    }
}