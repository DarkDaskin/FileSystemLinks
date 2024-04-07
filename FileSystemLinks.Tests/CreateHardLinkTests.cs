namespace FileSystemLinks.Tests;

[TestClass]
public class CreateHardLinkTests : TestBase
{
    [TestMethod]
    public void WhenSourceAndDestFileNamesAreCorrectAndAbsolute_CreateHardLink()
    {
        var sourceFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        var destFileName = Path.Combine(WorkDirectoryPath, UnicodeString + Path.GetRandomFileName());
        File.WriteAllText(sourceFileName, "test");

        Assert.IsTrue(File.Exists(sourceFileName));
        Assert.IsFalse(File.Exists(destFileName));

        FileSystemLink.CreateHardLink(sourceFileName, destFileName);

        Assert.IsTrue(File.Exists(sourceFileName));
        Assert.IsTrue(File.Exists(destFileName));

        File.WriteAllText(sourceFileName, "test2");

        Assert.AreEqual("test2", File.ReadAllText(destFileName));
    }

    [TestMethod]
    public void WhenSourceAndDestFileNamesAreCorrectAndRelative_CreateHardLink()
    {
        var sourceFileName = UnicodeString + Path.GetRandomFileName();
        var destFileName = UnicodeString + Path.GetRandomFileName();
        File.WriteAllText(sourceFileName, "test");

        Assert.IsTrue(File.Exists(sourceFileName));
        Assert.IsFalse(File.Exists(destFileName));

        FileSystemLink.CreateHardLink(sourceFileName, destFileName);

        Assert.IsTrue(File.Exists(sourceFileName));
        Assert.IsTrue(File.Exists(destFileName));

        File.WriteAllText(sourceFileName, "test2");

        Assert.AreEqual("test2", File.ReadAllText(destFileName));
    }

    [TestMethod, ExpectedException(typeof(ArgumentNullException))]
    public void WhenSourceFileNameIsNull_Throw()
    {
        var destFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());

        FileSystemLink.CreateHardLink(null!, destFileName);
    }

    [TestMethod, ExpectedException(typeof(ArgumentException))]
    public void WhenSourceFileNameIsEmpty_Throw()
    {
        var destFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());

        FileSystemLink.CreateHardLink("", destFileName);
    }

    [TestMethod, ExpectedException(typeof(ArgumentNullException))]
    public void WhenDestFileNameIsNull_Throw()
    {
        var sourceFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());

        FileSystemLink.CreateHardLink(sourceFileName, null!);
    }

    [TestMethod, ExpectedException(typeof(ArgumentException))]
    public void WhenDestFileNameIsEmpty_Throw()
    {
        var sourceFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());

        FileSystemLink.CreateHardLink(sourceFileName, "");
    }

    [TestMethod, ExpectedException(typeof(FileNotFoundException))]
    public void WhenSourceFileDoesNotExist_Throw()
    {
        var sourceFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());
        var destFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());

        Assert.IsFalse(File.Exists(sourceFileName));
        Assert.IsFalse(File.Exists(destFileName));

        FileSystemLink.CreateHardLink(sourceFileName, destFileName);
    }

    [TestMethod, ExpectedException(typeof(UnauthorizedAccessException))]
    public void WhenSourceFileIsADirectory_Throw()
    {
        var sourceFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());
        var destFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());
        Directory.CreateDirectory(sourceFileName);

        Assert.IsTrue(Directory.Exists(sourceFileName));
        Assert.IsFalse(File.Exists(destFileName));

        FileSystemLink.CreateHardLink(sourceFileName, destFileName);
    }

    [TestMethod, ExpectedException(typeof(IOException))]
    public void WhenDestFileExists_Throw()
    {
        var sourceFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());
        var destFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());
        File.WriteAllText(sourceFileName, "test");
        File.WriteAllText(destFileName, "test");

        Assert.IsTrue(File.Exists(sourceFileName));
        Assert.IsTrue(File.Exists(destFileName));

        FileSystemLink.CreateHardLink(sourceFileName, destFileName);
    }

    [TestMethod, ExpectedException(typeof(IOException))]
    public void WhenSourceAndDestFilesAreOnDifferentFilesystems_Throw()
    {
        if (DifferentFileSystemWorkDirectoryPath is null)
        {
            Assert.Inconclusive();
            return;
        }

        var sourceFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());
        var destFileName = Path.Combine(DifferentFileSystemWorkDirectoryPath, Path.GetRandomFileName());
        File.WriteAllText(sourceFileName, "test");

        Assert.IsTrue(File.Exists(sourceFileName));
        Assert.IsFalse(File.Exists(destFileName));

        FileSystemLink.CreateHardLink(sourceFileName, destFileName);
    }

    [TestMethod, ExpectedException(typeof(UnauthorizedAccessException))]
    public void WhenHasNoAccessToSourceFile_ThrowOnWindows()
    {
        if (!IsWindows())
            Assert.Inconclusive();

        var sourceFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());
        var destFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());
        File.WriteAllText(sourceFileName, "test");
        MakeFileInaccessible(sourceFileName);

        Assert.IsTrue(File.Exists(sourceFileName));
        Assert.IsFalse(File.Exists(destFileName));

        try
        {
            FileSystemLink.CreateHardLink(sourceFileName, destFileName);
        }
        finally
        {
            MakeFileAccessible(sourceFileName);
        }
    }

    [TestMethod, ExpectedException(typeof(UnauthorizedAccessException))]
    public void WhenHasNoAccessToDestDirectory_Throw()
    {
        var sourceFileName = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());
        var destDirectoryPath = Path.Combine(WorkDirectoryPath, Path.GetRandomFileName());
        Directory.CreateDirectory(destDirectoryPath);
        var destFileName = Path.Combine(destDirectoryPath, Path.GetRandomFileName());
        File.WriteAllText(sourceFileName, "test");
        MakeDirectoryInaccessible(destDirectoryPath);

        Assert.IsTrue(File.Exists(sourceFileName));
        Assert.IsFalse(File.Exists(destFileName));

        try
        {
            FileSystemLink.CreateHardLink(sourceFileName, destFileName);
        }
        finally
        {
            MakeDirectoryAccessible(destDirectoryPath);
        }
    }
}