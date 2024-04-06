namespace FileSystemLinks.Tests;

[TestClass]
public class ResolveFileLinkTargetTests : TestBase
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

        var returnedTarget = FileSystemLink.ResolveFileLinkTarget(linkFileName, false);

        Assert.IsInstanceOfType<FileInfo>(returnedTarget);
        Assert.AreEqual(intermediateLinkFileName, returnedTarget.FullName);
    }

    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetExistsAndIsAbsolute_ReturnFinalTarget()
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

        var returnedTarget = FileSystemLink.ResolveFileLinkTarget(linkFileName, true);

        Assert.IsInstanceOfType<FileInfo>(returnedTarget);
        Assert.AreEqual(targetFileName, returnedTarget.FullName);
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

        var returnedTarget = FileSystemLink.ResolveFileLinkTarget(linkFileName, false);

        Assert.IsInstanceOfType<FileInfo>(returnedTarget);
        Assert.AreEqual(Path.Combine(WorkDirectoryPath, intermediateLinkFileName), returnedTarget.FullName);
    }

    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetExistsAndIsRelative_ReturnFinalTarget()
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

        var returnedTarget = FileSystemLink.ResolveFileLinkTarget(linkFileName, true);

        Assert.IsInstanceOfType<FileInfo>(returnedTarget);
        Assert.AreEqual(Path.Combine(WorkDirectoryPath, targetFileName), returnedTarget.FullName);
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

        var returnedTarget = FileSystemLink.ResolveFileLinkTarget(linkFileName, true);

        Assert.IsInstanceOfType<FileInfo>(returnedTarget);
        Assert.AreEqual(targetFileName, returnedTarget.FullName);
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

        var returnedTarget = FileSystemLink.ResolveFileLinkTarget(linkFileName, true);

        Assert.IsInstanceOfType<FileInfo>(returnedTarget);
        Assert.AreEqual(Path.Combine(WorkDirectoryPath, targetFileName), returnedTarget.FullName);
    }

    [TestMethod, ExpectedException(typeof(UnauthorizedAccessException))]
    public void WhenLinkIsSymbolicAndTargetIsInaccessible_Throw()
    {
        var targetFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        File.WriteAllText(targetFileName, "test");
#if NET6_0_OR_GREATER
        File.CreateSymbolicLink(linkFileName, targetFileName);
#else
        FileSystemLink.CreateFileSymbolicLink(linkFileName, targetFileName);
#endif
        MakeFileInaccessible(targetFileName);

        try
        {
            FileSystemLink.ResolveFileLinkTarget(linkFileName, true);
        }
        finally
        {
            MakeFileAccessible(targetFileName);
        }
    }

    [TestMethod]
    public void WhenLinkIsSymbolicAndTargetIsDirectory_ReturnTarget()
    {
        var targetFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var linkFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        Directory.CreateDirectory(targetFileName);
#if NET6_0_OR_GREATER
        Directory.CreateSymbolicLink(linkFileName, targetFileName);
#else
        FileSystemLink.CreateDirectorySymbolicLink(linkFileName, targetFileName);
#endif

        var returnedTarget = FileSystemLink.ResolveFileLinkTarget(linkFileName, true);

        Assert.IsInstanceOfType<FileInfo>(returnedTarget);
        Assert.AreEqual(targetFileName, returnedTarget.FullName);
    }

    [DataTestMethod, DataRow(false), DataRow(true)]
    public void WhenPathIsPlainFile_ReturnNull(bool returnFinalTarget)
    {
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        File.WriteAllText(linkDirectoryPath, "test");

        var returnedTarget = FileSystemLink.ResolveFileLinkTarget(linkDirectoryPath, returnFinalTarget);

        Assert.IsNull(returnedTarget);
    }

    [DataTestMethod, DataRow(false), DataRow(true)]
    public void WhenPathIsPlainDirectory_ReturnNull(bool returnFinalTarget)
    {
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        Directory.CreateDirectory(linkDirectoryPath);

        var returnedTarget = FileSystemLink.ResolveFileLinkTarget(linkDirectoryPath, returnFinalTarget);

        Assert.IsNull(returnedTarget);
    }

    [DataTestMethod, DataRow(false), DataRow(true), ExpectedException(typeof(FileNotFoundException))]
    public void WhenPathDoesNotExist_Throw(bool returnFinalTarget)
    {
        var linkDirectoryPath = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());

        FileSystemLink.ResolveFileLinkTarget(linkDirectoryPath, returnFinalTarget);
    }
}