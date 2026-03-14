namespace FindProjSln.FileSystem
{
    public static class PathExtensions
    {

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
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
        }

    }
}
