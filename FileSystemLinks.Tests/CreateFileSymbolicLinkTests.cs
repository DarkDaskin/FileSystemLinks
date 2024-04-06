namespace FileSystemLinks.Tests;

[TestClass]
public class CreateFileSymbolicLinkTests : TestBase
{
    [TestMethod]
    public void WhenPathsAreCorrectAndAbsolutePathTargetExists_CreateSymbolicLink()
    {
        var targetFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        File.WriteAllText(targetFileName, "test");

        Assert.IsTrue(File.Exists(targetFileName));
        Assert.IsFalse(File.Exists(linkFileName));

        FileSystemLink.CreateFileSymbolicLink(linkFileName, targetFileName);
        
        var link = new FileInfo(linkFileName);

        Assert.IsTrue(link.Exists);
#if NET6_0_OR_GREATER
        Assert.AreEqual(targetFileName, link.LinkTarget);
#else
        Assert.AreEqual(targetFileName, FileSystemLink.GetFileLinkTarget(linkFileName));
#endif

        File.WriteAllText(targetFileName, "test2");

        Assert.AreEqual("test2", File.ReadAllText(linkFileName));
    }

    [TestMethod]
    public void WhenPathsAreCorrectAndAbsolutePathTargetDoesNotExist_CreateSymbolicLink()
    {
        var targetFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());

        Assert.IsFalse(File.Exists(targetFileName));
        Assert.IsFalse(File.Exists(linkFileName));

        FileSystemLink.CreateFileSymbolicLink(linkFileName, targetFileName);
        
        var link = new FileInfo(linkFileName);

        Assert.IsTrue(link.Exists);
#if NET6_0_OR_GREATER
        Assert.AreEqual(targetFileName, link.LinkTarget);
#else
        Assert.AreEqual(targetFileName, FileSystemLink.GetFileLinkTarget(linkFileName));
#endif

        File.WriteAllText(targetFileName, "test2");

        Assert.AreEqual("test2", File.ReadAllText(linkFileName));
    }

    [TestMethod]
    public void WhenPathsAreCorrectAndRelativePathTargetExists_CreateSymbolicLink()
    {
        var targetFileName = UnicodeString + Path.GetRandomFileName();
        var linkFileName = UnicodeString + Path.GetRandomFileName();
        File.WriteAllText(targetFileName, "test");

        Assert.IsTrue(File.Exists(targetFileName));
        Assert.IsFalse(File.Exists(linkFileName));

        FileSystemLink.CreateFileSymbolicLink(linkFileName, targetFileName);
        
        var link = new FileInfo(linkFileName);

        Assert.IsTrue(link.Exists);
#if NET6_0_OR_GREATER
        Assert.AreEqual(targetFileName, link.LinkTarget);
#else
        Assert.AreEqual(targetFileName, FileSystemLink.GetFileLinkTarget(linkFileName));
#endif

        File.WriteAllText(targetFileName, "test2");

        Assert.AreEqual("test2", File.ReadAllText(linkFileName));
    }

    [TestMethod]
    public void WhenPathsAreCorrectAndRelativePathTargetDoesNotExist_CreateSymbolicLink()
    {
        var targetFileName = UnicodeString + Path.GetRandomFileName();
        var linkFileName = UnicodeString + Path.GetRandomFileName();

        Assert.IsFalse(File.Exists(targetFileName));
        Assert.IsFalse(File.Exists(linkFileName));

        FileSystemLink.CreateFileSymbolicLink(linkFileName, targetFileName);
        
        var link = new FileInfo(linkFileName);

        Assert.IsTrue(link.Exists);
#if NET6_0_OR_GREATER
        Assert.AreEqual(targetFileName, link.LinkTarget);
#else
        Assert.AreEqual(targetFileName, FileSystemLink.GetFileLinkTarget(linkFileName));
#endif

        File.WriteAllText(targetFileName, "test2");

        Assert.AreEqual("test2", File.ReadAllText(linkFileName));
    }

    [TestMethod]
    public void WhenPathsAreCorrectAndTargetIsUnc_CreateSymbolicLinkOnWindows()
    {
        if (!IsWindows())
        {
            Assert.Inconclusive();
            return;
        }

        var targetFileName = @"\\localhost\c$\file";
        var linkFileName = UnicodeString + Path.GetRandomFileName();

        Assert.IsFalse(File.Exists(linkFileName));

        FileSystemLink.CreateFileSymbolicLink(linkFileName, targetFileName);
        
        var link = new FileInfo(linkFileName);

        Assert.IsTrue(link.Exists);
#if NET6_0_OR_GREATER
        Assert.AreEqual(targetFileName, link.LinkTarget);
#else
        Assert.AreEqual(targetFileName, FileSystemLink.GetFileLinkTarget(linkFileName));
#endif
    }

    [TestMethod, ExpectedException(typeof(ArgumentNullException))]
    public void WhenPathIsNull_Throw()
    {
        var pathToTarget = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());

        FileSystemLink.CreateFileSymbolicLink(null!, pathToTarget);
    }

    [TestMethod, ExpectedException(typeof(ArgumentException))]
    public void WhenPathIsEmpty_Throw()
    {
        var pathToTarget = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());

        FileSystemLink.CreateFileSymbolicLink("", pathToTarget);
    }

    [TestMethod, ExpectedException(typeof(ArgumentNullException))]
    public void WhenPathToTargetIsNull_Throw()
    {
        var path = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());

        FileSystemLink.CreateFileSymbolicLink(path, null!);
    }

    [TestMethod, ExpectedException(typeof(ArgumentException))]
    public void WhenPathToTargetIsEmpty_Throw()
    {
        var path = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());

        FileSystemLink.CreateFileSymbolicLink(path, "");
    }

    [TestMethod, ExpectedException(typeof(IOException))]
    public void WhenFileAtPathExists_Throw()
    {
        var targetFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());
        var linkFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());
        File.WriteAllText(linkFileName, "test");

        Assert.IsFalse(File.Exists(targetFileName));
        Assert.IsTrue(File.Exists(linkFileName));

        FileSystemLink.CreateFileSymbolicLink(linkFileName, targetFileName);
    }

    [TestMethod, ExpectedException(typeof(UnauthorizedAccessException))]
    public void WhenHasNoAccessToPath_Throw()
    {
        var targetFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());
        Directory.CreateDirectory(linkDirectoryPath);
        var linkFileName = Path.Combine(linkDirectoryPath, Path.GetRandomFileName());
        MakeDirectoryInaccessible(linkDirectoryPath);

        try
        {
            FileSystemLink.CreateFileSymbolicLink(linkFileName, targetFileName);
        }
        finally
        {
            MakeDirectoryAccessible(linkDirectoryPath);
        }
    }
}