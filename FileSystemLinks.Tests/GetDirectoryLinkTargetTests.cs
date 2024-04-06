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
        Directory.CreateSymbolicLink(intermediateLinkDirectoryPath, targetDirectoryPath);
        Directory.CreateSymbolicLink(linkDirectoryPath, intermediateLinkDirectoryPath);
#else
        FileSystemLink.CreateDirectorySymbolicLink(intermediateLinkDirectoryPath, targetDirectoryPath);
        FileSystemLink.CreateDirectorySymbolicLink(linkDirectoryPath, intermediateLinkDirectoryPath);
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
        Directory.CreateSymbolicLink(intermediateLinkDirectoryPath, targetDirectoryPath);
        Directory.CreateSymbolicLink(linkDirectoryPath, intermediateLinkDirectoryPath);
#else
        FileSystemLink.CreateDirectorySymbolicLink(intermediateLinkDirectoryPath, targetDirectoryPath);
        FileSystemLink.CreateDirectorySymbolicLink(linkDirectoryPath, intermediateLinkDirectoryPath);
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
        Directory.CreateSymbolicLink(linkDirectoryPath, targetDirectoryPath);
#else
        FileSystemLink.CreateDirectorySymbolicLink(linkDirectoryPath, targetDirectoryPath);
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
        Directory.CreateSymbolicLink(linkDirectoryPath, targetDirectoryPath);
#else
        FileSystemLink.CreateDirectorySymbolicLink(linkDirectoryPath, targetDirectoryPath);
#endif

        var returnedTarget = FileSystemLink.GetFileLinkTarget(linkDirectoryPath);

        Assert.AreEqual(targetDirectoryPath, returnedTarget);
    }

    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetIsRelativeUpper_ReturnTarget()
    {
        var targetDirectoryPath = UnicodeString + Path.GetRandomFileName();
        var linkSubDirectory = Path.GetRandomFileName();
        var linkDirectoryPath = Path.Combine(linkSubDirectory, UnicodeString + Path.GetRandomFileName());
        var targetRelativeDirectoryPath = Path.Combine("..", targetDirectoryPath);
        Directory.CreateDirectory(linkSubDirectory);
#if NET6_0_OR_GREATER
        Directory.CreateSymbolicLink(linkDirectoryPath, targetRelativeDirectoryPath);
#else
        FileSystemLink.CreateDirectorySymbolicLink(linkDirectoryPath, targetRelativeDirectoryPath);
#endif

        var returnedTarget = FileSystemLink.GetDirectoryLinkTarget(linkDirectoryPath);

        Assert.AreEqual(targetRelativeDirectoryPath, returnedTarget);
    }

    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetIsInaccessible_ReturnTarget()
    {
        var targetDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        Directory.CreateDirectory(targetDirectoryPath);
#if NET6_0_OR_GREATER
        Directory.CreateSymbolicLink(linkDirectoryPath, targetDirectoryPath);
#else
        FileSystemLink.CreateDirectorySymbolicLink(linkDirectoryPath, targetDirectoryPath);
#endif
        MakeFileInaccessible(targetDirectoryPath);

        try
        {
            var returnedTarget = FileSystemLink.GetFileLinkTarget(linkDirectoryPath);

            Assert.AreEqual(targetDirectoryPath, returnedTarget);
        }
        finally
        {
            MakeFileAccessible(targetDirectoryPath);
        }
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