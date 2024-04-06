namespace FileSystemLinks.Tests;

[TestClass]
public class ResolveDirectoryLinkTargetTests : TestBase
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

        var returnedTarget = FileSystemLink.ResolveDirectoryLinkTarget(linkDirectoryPath, false);

        Assert.IsInstanceOfType<DirectoryInfo>(returnedTarget);
        Assert.AreEqual(intermediateLinkDirectoryPath, returnedTarget.FullName);
    }

    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetExistsAndIsAbsolute_ReturnFinalTarget()
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

        var returnedTarget = FileSystemLink.ResolveDirectoryLinkTarget(linkDirectoryPath, true);

        Assert.IsInstanceOfType<DirectoryInfo>(returnedTarget);
        Assert.AreEqual(targetDirectoryPath, returnedTarget.FullName);
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

        var returnedTarget = FileSystemLink.ResolveDirectoryLinkTarget(linkDirectoryPath, false);

        Assert.IsInstanceOfType<DirectoryInfo>(returnedTarget);
        Assert.AreEqual(Path.Combine(WorkDirectoryPath, intermediateLinkDirectoryPath), returnedTarget.FullName);
    }

    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetExistsAndIsRelative_ReturnFinalTarget()
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

        var returnedTarget = FileSystemLink.ResolveDirectoryLinkTarget(linkDirectoryPath, true);

        Assert.IsInstanceOfType<DirectoryInfo>(returnedTarget);
        Assert.AreEqual(Path.Combine(WorkDirectoryPath, targetDirectoryPath), returnedTarget.FullName);
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

        var returnedTarget = FileSystemLink.ResolveDirectoryLinkTarget(linkDirectoryPath, true);

        Assert.IsInstanceOfType<DirectoryInfo>(returnedTarget);
        Assert.AreEqual(targetDirectoryPath, returnedTarget.FullName);
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

        var returnedTarget = FileSystemLink.ResolveDirectoryLinkTarget(linkDirectoryPath, true);

        Assert.IsInstanceOfType<DirectoryInfo>(returnedTarget);
        Assert.AreEqual(Path.Combine(WorkDirectoryPath, targetDirectoryPath), returnedTarget.FullName);
    }

    [TestMethod, ExpectedException(typeof(UnauthorizedAccessException))]
    public void WhenLinkIsSymbolicAndTargetIsInaccessible_Throw()
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
            FileSystemLink.ResolveDirectoryLinkTarget(linkDirectoryPath, true);
        }
        finally
        {
            MakeFileAccessible(targetDirectoryPath);
        }
    }

    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetIsFile_ReturnTarget()
    {
        var targetDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        File.WriteAllText(targetDirectoryPath, "test");
#if NET6_0_OR_GREATER
        File.CreateSymbolicLink(linkDirectoryPath, targetDirectoryPath);
#else
        FileSystemLink.CreateFileSymbolicLink(linkDirectoryPath, targetDirectoryPath);
#endif

        var returnedTarget = FileSystemLink.ResolveDirectoryLinkTarget(linkDirectoryPath, true);

        Assert.IsInstanceOfType<DirectoryInfo>(returnedTarget);
        Assert.AreEqual(targetDirectoryPath, returnedTarget.FullName);
    }

    [TestMethod]
    public void WhenLinkIsJunctionAndTargetExists_ReturnFirstTarget()
    {
        if (!IsWindows())
        {
            Assert.Inconclusive();
            return;
        }

        var targetDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var intermediateLinkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        Directory.CreateDirectory(targetDirectoryPath);
        FileSystemLink.CreateJunction(intermediateLinkDirectoryPath, targetDirectoryPath);
        FileSystemLink.CreateJunction(linkDirectoryPath, intermediateLinkDirectoryPath);

        var returnedTarget = FileSystemLink.ResolveDirectoryLinkTarget(linkDirectoryPath, false);

        Assert.IsInstanceOfType<DirectoryInfo>(returnedTarget);
        Assert.AreEqual(intermediateLinkDirectoryPath, returnedTarget.FullName);
    }

    [TestMethod]
    public void WhenLinkIsJunctionAndTargetExists_ReturnFinalTarget()
    {
        if (!IsWindows())
        {
            Assert.Inconclusive();
            return;
        }

        var targetDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var intermediateLinkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        Directory.CreateDirectory(targetDirectoryPath);
        FileSystemLink.CreateJunction(intermediateLinkDirectoryPath, targetDirectoryPath);
        FileSystemLink.CreateJunction(linkDirectoryPath, intermediateLinkDirectoryPath);

        var returnedTarget = FileSystemLink.ResolveDirectoryLinkTarget(linkDirectoryPath, true);

        Assert.IsInstanceOfType<DirectoryInfo>(returnedTarget);
        Assert.AreEqual(targetDirectoryPath, returnedTarget.FullName);
    }

    [TestMethod]
    public void WhenLinkIsJunctionAndTargetDoesNotExist_ReturnTarget()
    {
        if (!IsWindows())
        {
            Assert.Inconclusive();
            return;
        }

        var targetDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        FileSystemLink.CreateJunction(linkDirectoryPath, targetDirectoryPath);

        var returnedTarget = FileSystemLink.ResolveDirectoryLinkTarget(linkDirectoryPath, false);

        Assert.IsInstanceOfType<DirectoryInfo>(returnedTarget);
        Assert.AreEqual(targetDirectoryPath, returnedTarget.FullName);
    }

    [DataTestMethod, DataRow(false), DataRow(true)]
    public void WhenPathIsPlainFile_ReturnNull(bool returnFinalTarget)
    {
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        File.WriteAllText(linkDirectoryPath, "test");

        var returnedTarget = FileSystemLink.ResolveDirectoryLinkTarget(linkDirectoryPath, returnFinalTarget);

        Assert.IsNull(returnedTarget);
    }

    [DataTestMethod, DataRow(false), DataRow(true)]
    public void WhenPathIsPlainDirectory_ReturnNull(bool returnFinalTarget)
    {
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        Directory.CreateDirectory(linkDirectoryPath);

        var returnedTarget = FileSystemLink.ResolveDirectoryLinkTarget(linkDirectoryPath, returnFinalTarget);

        Assert.IsNull(returnedTarget);
    }

    [DataTestMethod, DataRow(false), DataRow(true), ExpectedException(typeof(DirectoryNotFoundException))]
    public void WhenPathDoesNotExist_Throw(bool returnFinalTarget)
    {
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());

        FileSystemLink.ResolveDirectoryLinkTarget(linkDirectoryPath, returnFinalTarget);
    }
}