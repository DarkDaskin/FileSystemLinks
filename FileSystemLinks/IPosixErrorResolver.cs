namespace FileSystemLinks;

internal interface IPosixErrorResolver
{
    PosixError GetErrorFromNativeErrorCode(int errorCode);
    int GetNativeErrorCodeFromError(PosixError error);
    string GetStringFromNativeErrorCode(int errorCode);
}