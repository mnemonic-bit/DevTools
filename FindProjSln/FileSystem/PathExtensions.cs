namespace FindProjSln.FileSystem
{
    public static class PathExtensions
    {

        public static IEnumerable<string> EnumerateAllSubDirectories(this string path)
        {
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true
            };

            return Directory.EnumerateDirectories(path, "*", options)
                .Prepend(path);
        }

        public static IEnumerable<string> EnumerateDirectoriesAlongPath(this string path)
        {
            var currentDirectory = path;
            while (currentDirectory is not null)
            {
                yield return currentDirectory;

                var parent = Path.GetDirectoryName(currentDirectory)!;
                currentDirectory = parent == currentDirectory ? null : parent;
            }
        }

        public static bool IsSeparatorChar(this char ch)
        {
            return ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar;
        }

        public static bool IsUnderDirectory(this string filePath, string dirPath)
        {
            string normalizedFileName = Path.GetFullPath(filePath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string normalizedDirectoryName = Path.GetFullPath(dirPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

            return normalizedFileName.StartsWith(normalizedDirectoryName, StringComparison.OrdinalIgnoreCase);
        }

        public static string NormalizeSeparatorChars(this string path)
        {
            return path
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

    }
}
