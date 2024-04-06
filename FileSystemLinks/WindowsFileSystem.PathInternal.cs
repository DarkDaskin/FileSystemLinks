using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FileSystemLinks;

internal partial class WindowsFileSystem
{
    // Contains portions of https://github.com/dotnet/runtime/blob/v8.0.3/src/libraries/Common/src/System/IO/PathInternal.Windows.cs
    private static class PathInternal
    {
        private const char DirectorySeparatorChar = '\\';
        private const char AltDirectorySeparatorChar = '/';
        private const char VolumeSeparatorChar = ':';
        // ReSharper disable once InconsistentNaming
        public const string NTPathPrefix = @"\??\";
        private const string ExtendedPathPrefix = @"\\?\";
        public const string UncPathPrefix = @"\\";
        private const string UncExtendedPrefixToInsert = @"?\UNC\";
        // ReSharper disable once InconsistentNaming
        public const string UncNTPathPrefix = @"\??\UNC\";
        private const int MaxShortPath = 260;
        // \\?\, \\.\, \??\
        private const int DevicePrefixLength = 4;
        // \\
        private const int UncPrefixLength = 2;
        // \\?\UNC\, \\.\UNC\
        private const int UncExtendedPrefixLength = 8;

        /// <summary>
        /// Adds the extended path prefix (\\?\) if not already a device path, IF the path is not relative,
        /// AND the path is more than 259 characters. (> MAX_PATH + null). This will also insert the extended
        /// prefix if the path ends with a period or a space. Trailing periods and spaces are normally eaten
        /// away from paths during normalization, but if we see such a path at this point it should be
        /// normalized and has retained the final characters. (Typically from one of the *Info classes)
        /// </summary>
        [return: NotNullIfNotNull(nameof(path))]
        public static string? EnsureExtendedPrefixIfNeeded(string? path)
        {
            if (path != null && (path.Length >= MaxShortPath || EndsWithPeriodOrSpace(path)))
            {
                return EnsureExtendedPrefix(path);
            }
            else
            {
                return path;
            }
        }

        private static bool EndsWithPeriodOrSpace(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            char c = path![path.Length - 1];
            return c == ' ' || c == '.';
        }

        /// <summary>
        /// Adds the extended path prefix (\\?\) if not relative or already a device path.
        /// </summary>
        private static string EnsureExtendedPrefix(string path)
        {
            // Putting the extended prefix on the path changes the processing of the path. It won't get normalized, which
            // means adding to relative paths will prevent them from getting the appropriate current directory inserted.

            // If it already has some variant of a device path (\??\, \\?\, \\.\, //./, etc.) we don't need to change it
            // as it is either correct or we will be changing the behavior. When/if Windows supports long paths implicitly
            // in the future we wouldn't want normalization to come back and break existing code.

            // In any case, all internal usages should be hitting normalize path (Path.GetFullPath) before they hit this
            // shimming method. (Or making a change that doesn't impact normalization, such as adding a filename to a
            // normalized base path.)
            if (IsPartiallyQualified(path) || IsDevice(path))
                return path;

            // Given \\server\share in longpath becomes \\?\UNC\server\share
            if (path.StartsWith(UncPathPrefix, StringComparison.OrdinalIgnoreCase))
                return path.Insert(2, UncExtendedPrefixToInsert);

            return ExtendedPathPrefix + path;
        }

        /// <summary>
        /// Returns true if the path specified is relative to the current drive or working directory.
        /// Returns false if the path is fixed to a specific drive or UNC path.  This method does no
        /// validation of the path (URIs will be returned as relative as a result).
        /// </summary>
        /// <remarks>
        /// Handles paths that use the alternate directory separator.  It is a frequent mistake to
        /// assume that rooted paths (Path.IsPathRooted) are not relative.  This isn't the case.
        /// "C:a" is drive relative- meaning that it will be resolved against the current directory
        /// for C: (rooted, but relative). "C:\a" is rooted and not relative (the current directory
        /// will not be used to modify the path).
        /// </remarks>
        public static bool IsPartiallyQualified(string path)
        {
            if (path.Length < 2)
            {
                // It isn't fixed, it must be relative.  There is no way to specify a fixed
                // path with one character (or less).
                return true;
            }

            if (IsDirectorySeparator(path[0]))
            {
                // There is no valid way to specify a relative path with two initial slashes or
                // \? as ? isn't valid for drive relative paths and \??\ is equivalent to \\?\
                return !(path[1] == '?' || IsDirectorySeparator(path[1]));
            }

            // The only way to specify a fixed path that doesn't begin with two slashes
            // is the drive, colon, slash format- i.e. C:\
            return !((path.Length >= 3)
                && (path[1] == VolumeSeparatorChar)
                && IsDirectorySeparator(path[2])
                // To match old behavior we'll check the drive character for validity as the path is technically
                // not qualified if you don't have a valid drive. "=:\" is the "=" file's default data stream.
                && IsValidDriveChar(path[0]));
        }

        /// <summary>
        /// True if the given character is a directory separator.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDirectorySeparator(char c)
        {
            return c == DirectorySeparatorChar || c == AltDirectorySeparatorChar;
        }

        /// <summary>
        /// Returns true if the given character is a valid drive letter
        /// </summary>
        private static bool IsValidDriveChar(char value)
        {
            return (uint)((value | 0x20) - 'a') <= (uint)('z' - 'a');
        }

        /// <summary>
        /// Returns true if the path uses any of the DOS device path syntaxes. ("\\.\", "\\?\", or "\??\")
        /// </summary>
        private static bool IsDevice(string path)
        {
            // If the path begins with any two separators is will be recognized and normalized and prepped with
            // "\??\" for internal usage correctly. "\??\" is recognized and handled, "/??/" is not.
            return IsExtended(path)
                   ||
                   (
                       path.Length >= DevicePrefixLength
                       && IsDirectorySeparator(path[0])
                       && IsDirectorySeparator(path[1])
                       && (path[2] == '.' || path[2] == '?')
                       && IsDirectorySeparator(path[3])
                   );
        }

        /// <summary>
        /// Returns true if the path is a device UNC (\\?\UNC\, \\.\UNC\)
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private static bool IsDeviceUNC(string path)
        {
            return path.Length >= UncExtendedPrefixLength
                   && IsDevice(path)
                   && IsDirectorySeparator(path[7])
                   && path[4] == 'U'
                   && path[5] == 'N'
                   && path[6] == 'C';
        }

        /// <summary>
        /// Returns true if the path uses the canonical form of extended syntax ("\\?\" or "\??\"). If the
        /// path matches exactly (cannot use alternate directory separators) Windows will skip normalization
        /// and path length checks.
        /// </summary>
        public static bool IsExtended(string path)
        {
            // While paths like "//?/C:/" will work, they're treated the same as "\\.\" paths.
            // Skipping of normalization will *only* occur if back slashes ('\') are used.
            return path.Length >= DevicePrefixLength
                   && path[0] == '\\'
                   && (path[1] == '\\' || path[1] == '?')
                   && path[2] == '?'
                   && path[3] == '\\';
        }

        /// <summary>
        /// Trims one trailing directory separator beyond the root of the path.
        /// </summary>
        [return: NotNullIfNotNull(nameof(path))]
        public static string? TrimEndingDirectorySeparator(string? path) =>
            EndsInDirectorySeparator(path) && !IsRoot(path) ?
                path!.Substring(0, path.Length - 1) :
                path;

        /// <summary>
        /// Returns true if the path ends in a directory separator.
        /// </summary>
        private static bool EndsInDirectorySeparator([NotNullWhen(true)] string? path) =>
            !string.IsNullOrEmpty(path) && IsDirectorySeparator(path![path.Length - 1]);

        private static bool IsRoot(string path)
            => path.Length == GetRootLength(path);

        /// <summary>
        /// Gets the length of the root of the path (drive, share, etc.).
        /// </summary>
        private static int GetRootLength(string path)
        {
            int pathLength = path.Length;
            int i = 0;

            bool deviceSyntax = IsDevice(path);
            bool deviceUnc = deviceSyntax && IsDeviceUNC(path);

            if ((!deviceSyntax || deviceUnc) && pathLength > 0 && IsDirectorySeparator(path[0]))
            {
                // UNC or simple rooted path (e.g. "\foo", NOT "\\?\C:\foo")
                if (deviceUnc || (pathLength > 1 && IsDirectorySeparator(path[1])))
                {
                    // UNC (\\?\UNC\ or \\), scan past server\share

                    // Start past the prefix ("\\" or "\\?\UNC\")
                    i = deviceUnc ? UncExtendedPrefixLength : UncPrefixLength;

                    // Skip two separators at most
                    int n = 2;
                    while (i < pathLength && (!IsDirectorySeparator(path[i]) || --n > 0))
                        i++;
                }
                else
                {
                    // Current drive rooted (e.g. "\foo")
                    i = 1;
                }
            }
            else if (deviceSyntax)
            {
                // Device path (e.g. "\\?\.", "\\.\")
                // Skip any characters following the prefix that aren't a separator
                i = DevicePrefixLength;
                while (i < pathLength && !IsDirectorySeparator(path[i]))
                    i++;

                // If there is another separator take it, as long as we have had at least one
                // non-separator after the prefix (e.g. don't take "\\?\\", but take "\\?\a\")
                if (i < pathLength && i > DevicePrefixLength && IsDirectorySeparator(path[i]))
                    i++;
            }
            else if (pathLength >= 2
                && path[1] == VolumeSeparatorChar
                && IsValidDriveChar(path[0]))
            {
                // Valid drive specified path ("C:", "D:", etc.)
                i = 2;

                // If the colon is followed by a directory separator, move past it (e.g "C:\")
                if (pathLength > 2 && IsDirectorySeparator(path[2]))
                    i++;
            }

            return i;
        }
    }
}
