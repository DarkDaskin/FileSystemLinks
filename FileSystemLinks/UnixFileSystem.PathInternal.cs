using System.IO;

namespace FileSystemLinks;

internal partial class UnixFileSystem
{
    private static class PathInternal
    {
        public const char DirectorySeparatorChar = '/';

        public static bool IsPartiallyQualified(string path)
        {
            // This is much simpler than Windows where paths can be rooted, but not fully qualified (such as Drive Relative)
            // As long as the path is rooted in Unix it doesn't use the current directory and therefore is fully qualified.
            return !Path.IsPathRooted(path);
        }

        public static int GetDirectoryNameOffset(string path)
        {
            int rootLength = GetRootLength(path);
            int end = path.Length;
            if (end <= rootLength)
                return -1;

            while (end > rootLength && !IsDirectorySeparator(path[--end])) { }

            // Trim off any remaining separators (to deal with C:\foo\\bar)
            while (end > rootLength && IsDirectorySeparator(path[end - 1]))
                end--;

            return end;
        }

        private static int GetRootLength(string path)
        {
            return path.Length > 0 && IsDirectorySeparator(path[0]) ? 1 : 0;
        }

        private static bool IsDirectorySeparator(char c)
        {
            return c == DirectorySeparatorChar;
        }
    }
}
