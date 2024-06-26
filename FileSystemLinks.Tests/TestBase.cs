﻿using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;

namespace FileSystemLinks.Tests;

[TestClass]
public class TestBase
{
    protected const string UnicodeString = "тест实验";

    protected static string WorkDirectoryPath = null!;
    protected static string? DifferentFileSystemWorkDirectoryPath;
    private static string _workDirectoryBasePath = null!;

    [AssemblyInitialize]
    public static void Initialize(TestContext context)
    {
        _workDirectoryBasePath = WorkDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
#if NETCOREAPP1_0_OR_GREATER || NET462_OR_GREATER
        var longDirectoryName = new string('a', 200);
        WorkDirectoryPath = Path.Combine(WorkDirectoryPath, longDirectoryName, longDirectoryName);
#endif

        Directory.CreateDirectory(_workDirectoryBasePath);

        try
        {
            Directory.CreateDirectory(WorkDirectoryPath);
            Environment.CurrentDirectory = WorkDirectoryPath;
        }
        catch (PathTooLongException)
        {
            // On Windows only ReSharper could run tests with long paths, Visual Studio Test Explorer and dotnet test fail.
            Environment.CurrentDirectory = WorkDirectoryPath = _workDirectoryBasePath;
        }

        DifferentFileSystemWorkDirectoryPath = Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => @"E:\Temp\FileSystemLinksTests",
            PlatformID.Unix => "/mnt/e/Temp/FileSystemLinksTests",
            _ => null
        };
        if (!Directory.Exists(DifferentFileSystemWorkDirectoryPath))
            DifferentFileSystemWorkDirectoryPath = null;
    }

    [AssemblyCleanup]
    public static void Cleanup()
    {
        Environment.CurrentDirectory = Path.GetTempPath();

        foreach (var directory in new DirectoryInfo(WorkDirectoryPath).EnumerateDirectories("*", SearchOption.AllDirectories))
            if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
                directory.Delete();

        Directory.Delete(_workDirectoryBasePath, true);

        if (DifferentFileSystemWorkDirectoryPath is not null)
        {
            foreach (var path in Directory.EnumerateDirectories(DifferentFileSystemWorkDirectoryPath))
                Directory.Delete(path, true);
            foreach (var path in Directory.EnumerateFiles(DifferentFileSystemWorkDirectoryPath))
                File.Delete(path);
        }
    }

#if NET5_0_OR_GREATER
    [SupportedOSPlatformGuard("windows")]
#endif
    protected static bool IsWindows()
    {
#if NET5_0_OR_GREATER
        return OperatingSystem.IsWindows();
#elif NETFRAMEWORK
        return Environment.OSVersion.Platform == PlatformID.Win32NT;
#else
        Assert.Inconclusive();
        return false;
#endif
    }

    protected static void MakeFileInaccessible(string fileName)
    {
#if NET7_0_OR_GREATER
        if (OperatingSystem.IsWindows())
        {
            var file = new FileInfo(fileName);
            var fileSecurity = file.GetAccessControl();
            fileSecurity.SetAccessRule(GetDenyAccessRule());
            file.SetAccessControl(fileSecurity);
        }
        else
            File.SetUnixFileMode(fileName, UnixFileMode.None);
#elif NETFRAMEWORK
        if (IsWindows())
        {
            var fileSecurity = File.GetAccessControl(fileName);
            fileSecurity.SetAccessRule(GetDenyAccessRule());
            File.SetAccessControl(fileName, fileSecurity);            
        }
        else
            Assert.Inconclusive();
#else
        Assert.Inconclusive();
#endif
    }

    protected static void MakeFileAccessible(string fileName)
    {
#if NET7_0_OR_GREATER
        if (OperatingSystem.IsWindows())
        {
            var file = new FileInfo(fileName);
            var fileSecurity = file.GetAccessControl();
            fileSecurity.RemoveAccessRule(GetDenyAccessRule());
            file.SetAccessControl(fileSecurity);
        }
        else
            File.SetUnixFileMode(fileName, AllFileAccess);
#elif NETFRAMEWORK
        if (IsWindows())
        {
            var fileSecurity = File.GetAccessControl(fileName);
            fileSecurity.RemoveAccessRule(GetDenyAccessRule());
            File.SetAccessControl(fileName, fileSecurity);
        }
        else
            Assert.Inconclusive();
#else
        Assert.Inconclusive();
#endif
    }

    protected static void MakeDirectoryInaccessible(string path)
    {
#if NET7_0_OR_GREATER
        if (OperatingSystem.IsWindows())
        {
            var directory = new DirectoryInfo(path);
            var directorySecurity = directory.GetAccessControl();
            directorySecurity.SetAccessRule(GetDenyAccessRule());
            directory.SetAccessControl(directorySecurity);
        }
        else
            File.SetUnixFileMode(path, UnixFileMode.None);
#elif NETFRAMEWORK
        if (IsWindows())
        {
            var directorySecurity = Directory.GetAccessControl(path);
            directorySecurity.SetAccessRule(GetDenyAccessRule());
            Directory.SetAccessControl(path, directorySecurity);
        }
        else
            Assert.Inconclusive();
#else
        Assert.Inconclusive();
#endif
    }

    protected static void MakeDirectoryAccessible(string path)
    {
#if NET7_0_OR_GREATER
        if (OperatingSystem.IsWindows())
        {
            var directory = new DirectoryInfo(path);
            var directorySecurity = directory.GetAccessControl();
            directorySecurity.RemoveAccessRule(GetDenyAccessRule());
            directory.SetAccessControl(directorySecurity);
        }
        else
            File.SetUnixFileMode(path, AllFileAccess);
#elif NETFRAMEWORK
        if (IsWindows())
        {
            var directorySecurity = Directory.GetAccessControl(path);
            directorySecurity.RemoveAccessRule(GetDenyAccessRule());
            Directory.SetAccessControl(path, directorySecurity);
        }
        else
            Assert.Inconclusive();
#else
        Assert.Inconclusive();
#endif
    }

#if NET7_0_OR_GREATER
    private const UnixFileMode AllFileAccess = UnixFileMode.UserRead | UnixFileMode.UserWrite;
#endif

#if NET7_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
#if NET7_0_OR_GREATER || NETFRAMEWORK
    private static FileSystemAccessRule GetDenyAccessRule() => new(WindowsIdentity.GetCurrent().User!,
        FileSystemRights.FullControl, AccessControlType.Deny);
#endif
}