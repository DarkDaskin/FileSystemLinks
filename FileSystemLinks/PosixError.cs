namespace FileSystemLinks;

/// <summary>Common Unix errno error codes.</summary>
// Values taken from https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/Interop/Unix/Interop.Errors.cs
internal enum PosixError
{
    // ReSharper disable InconsistentNaming
    EACCES = 0x10002,
    EBADF = 0x10008,
    EINVAL = 0x1001C,
    ELOOP = 0x10020,
    ENAMETOOLONG = 0x10025,
    ENOENT = 0x1002D,
    ENOTDIR = 0x10039,
    EPERM = 0x10042,
    // ReSharper restore InconsistentNaming
    UnknownError = 0x20000,
}