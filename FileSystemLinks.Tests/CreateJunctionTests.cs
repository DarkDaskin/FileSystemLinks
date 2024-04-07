namespace FileSystemLinks.Tests;

#pragma warning disable CA1416 // Validate platform compatibility - a check is made in TestInitialize.
[TestClass]
public class CreateJunctionTests : TestBase
{
    private const string TestFileName = "test.txt";

    [TestInitialize]
    public void TestInitialize()
    {
        if (!IsWindows()) 
            Assert.Inconclusive();
    }

    [TestMethod]
    public void WhenPathsAreCorrectAndAbsolutePathTargetExists_CreateJunction()
    {
        var targetDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        Directory.CreateDirectory(targetDirectoryPath);

        Assert.IsTrue(Directory.Exists(targetDirectoryPath));
        Assert.IsFalse(Directory.Exists(linkDirectoryPath));

        FileSystemLink.CreateJunction(linkDirectoryPath, targetDirectoryPath);
        
        var link = new DirectoryInfo(linkDirectoryPath);

        Assert.IsTrue(link.Exists);
#if NET6_0_OR_GREATER
        Assert.AreEqual(targetDirectoryPath, link.LinkTarget);
#else
        Assert.AreEqual(targetDirectoryPath, FileSystemLink.GetDirectoryLinkTarget(linkDirectoryPath));
#endif

        File.WriteAllText(Path.Combine(targetDirectoryPath, TestFileName), "test2");

        Assert.AreEqual("test2", File.ReadAllText(Path.Combine(linkDirectoryPath, TestFileName)));
    }

    [TestMethod]
    public void WhenPathsAreCorrectAndAbsolutePathTargetDoesNotExist_CreateJunction()
    {
        var targetDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());

        Assert.IsFalse(Directory.Exists(targetDirectoryPath));
        Assert.IsFalse(Directory.Exists(linkDirectoryPath));

        FileSystemLink.CreateJunction(linkDirectoryPath, targetDirectoryPath);

        var link = new DirectoryInfo(linkDirectoryPath);

        // Dangling symbolic links are considered files in Unix.
        Assert.IsTrue(IsWindows() ? link.Exists : File.Exists(linkDirectoryPath));
#if NET6_0_OR_GREATER
        Assert.AreEqual(targetDirectoryPath, link.LinkTarget);
#else
        Assert.AreEqual(targetDirectoryPath, FileSystemLink.GetDirectoryLinkTarget(linkDirectoryPath));
#endif

        Directory.CreateDirectory(targetDirectoryPath);
        File.WriteAllText(Path.Combine(targetDirectoryPath, TestFileName), "test2");

        Assert.AreEqual("test2", File.ReadAllText(Path.Combine(linkDirectoryPath, TestFileName)));
    }

    [TestMethod]
    public void WhenPathsAreCorrectAndRelativePathTargetExists_CreateJunction()
    {
        var targetDirectoryPath = UnicodeString + Path.GetRandomFileName();
        var linkDirectoryPath = UnicodeString + Path.GetRandomFileName();
        Directory.CreateDirectory(targetDirectoryPath);

        Assert.IsTrue(Directory.Exists(targetDirectoryPath));
        Assert.IsFalse(Directory.Exists(linkDirectoryPath));

        FileSystemLink.CreateJunction(linkDirectoryPath, targetDirectoryPath);

        var link = new DirectoryInfo(linkDirectoryPath);

        Assert.IsTrue(link.Exists);
#if NET6_0_OR_GREATER
        Assert.AreEqual(Path.Combine(WorkDirectoryPath, targetDirectoryPath), link.LinkTarget);
#else
        Assert.AreEqual(Path.Combine(WorkDirectoryPath, targetDirectoryPath), FileSystemLink.GetDirectoryLinkTarget(linkDirectoryPath));
#endif

        File.WriteAllText(Path.Combine(targetDirectoryPath, TestFileName), "test2");

        Assert.AreEqual("test2", File.ReadAllText(Path.Combine(linkDirectoryPath, TestFileName)));
    }

    [TestMethod]
    public void WhenPathsAreCorrectAndRelativePathTargetDoesNotExist_CreateJunction()
    {
        var targetDirectoryPath = UnicodeString + Path.GetRandomFileName();
        var linkDirectoryPath = UnicodeString + Path.GetRandomFileName();

        Assert.IsFalse(Directory.Exists(targetDirectoryPath));
        Assert.IsFalse(Directory.Exists(linkDirectoryPath));

        FileSystemLink.CreateJunction(linkDirectoryPath, targetDirectoryPath);

        var link = new DirectoryInfo(linkDirectoryPath);

        // Dangling symbolic links are considered files in Unix.
        Assert.IsTrue(IsWindows() ? link.Exists : File.Exists(linkDirectoryPath));
#if NET6_0_OR_GREATER
        Assert.AreEqual(Path.Combine(WorkDirectoryPath, targetDirectoryPath), link.LinkTarget);
#else
        Assert.AreEqual(Path.Combine(WorkDirectoryPath, targetDirectoryPath), FileSystemLink.GetDirectoryLinkTarget(linkDirectoryPath));
#endif

        Directory.CreateDirectory(targetDirectoryPath);
        File.WriteAllText(Path.Combine(targetDirectoryPath, TestFileName), "test2");

        Assert.AreEqual("test2", File.ReadAllText(Path.Combine(linkDirectoryPath, TestFileName)));
    }

    [TestMethod, ExpectedException(typeof(IOException))]
    public void WhenPathsAreCorrectAndTargetIsUnc_Throw()
    {
        var targetDirectoryPath = @"\\localhost\c$\directory";
        var linkDirectoryPath = UnicodeString + Path.GetRandomFileName();

        Assert.IsFalse(File.Exists(linkDirectoryPath));

        FileSystemLink.CreateJunction(linkDirectoryPath, targetDirectoryPath);

        var link = new DirectoryInfo(linkDirectoryPath);

        Assert.IsTrue(link.Exists);
#if NET6_0_OR_GREATER
        Assert.AreEqual(targetDirectoryPath, link.LinkTarget);
#else
        Assert.AreEqual(targetDirectoryPath, FileSystemLink.GetDirectoryLinkTarget(linkDirectoryPath));
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
        var targetDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        File.WriteAllText(linkDirectoryPath, "test");

        Assert.IsFalse(Directory.Exists(targetDirectoryPath));
        Assert.IsTrue(File.Exists(linkDirectoryPath));

        FileSystemLink.CreateJunction(linkDirectoryPath, targetDirectoryPath);
    }

    [TestMethod, ExpectedException(typeof(UnauthorizedAccessException))]
    public void WhenHasNoAccessToPath_Throw()
    {
        var targetDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkParentDirectoryPath = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());
        Directory.CreateDirectory(linkParentDirectoryPath);
        var linkDirectoryPath = Path.Combine(linkParentDirectoryPath, Path.GetRandomFileName());
        MakeDirectoryInaccessible(linkParentDirectoryPath);

        try
        {
            FileSystemLink.CreateJunction(linkDirectoryPath, targetDirectoryPath);
        }
        finally
        {
            MakeDirectoryAccessible(linkParentDirectoryPath);
        }
    }
}