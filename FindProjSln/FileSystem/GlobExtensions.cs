using System.Text.RegularExpressions;

namespace FindProjSln.FileSystem
{
    public static class GlobExtensions
    {

        public static bool GlobMatches(this string baseDir, string pattern, string targetPath)
        {
            pattern = pattern.NormalizeSeparatorChars();

            // Absolute pattern → compare directly
            if (Path.IsPathRooted(pattern))
            {
                string full = Path.GetFullPath(pattern);
                return string.Equals(full, targetPath, StringComparison.OrdinalIgnoreCase);
            }

            // No wildcard → exact relative path check
            if (!pattern.Contains('*') && !pattern.Contains('?'))
            {
                string full = Path.GetFullPath(Path.Combine(baseDir, pattern));
                return string.Equals(full, targetPath, StringComparison.OrdinalIgnoreCase);
            }

            // Wildcard: convert glob to regex and test against relative path from baseDir
            string relativePath = Path.GetRelativePath(baseDir, targetPath);
            string regex = GlobToRegex(pattern);

            return Regex.IsMatch(
                relativePath,
                regex,
                RegexOptions.IgnoreCase);
        }

        public static string GlobToRegex(string glob)
        {
            var result = new System.Text.StringBuilder("^");

            int pos = 0;
            while (pos < glob.Length)
            {
                // Check for **, which matches any path sequence
                if (glob[pos] == '*' && pos + 1 < glob.Length && glob[pos + 1] == '*')
                {
                    result.Append(".*");
                    pos += 2;
                    // skip trailing separator
                    if (pos < glob.Length && glob[pos].IsSeparatorChar()) pos++;
                }
                // Check for single *
                else if (glob[pos] == '*')
                {
                    // * matches anything except a separator
                    result.Append(GlobWildcardCharPatter);
                    pos++;
                }
                else if (glob[pos] == '?')
                {
                    result.Append('.');
                    pos++;
                }
                else
                {
                    result.Append(Regex.Escape(glob[pos].ToString()));
                    pos++;
                }
            }

            result.Append('$');

            return result.ToString();
        }


        private static string GlobWildcardCharPatter = "[^{Regex.Escape(Path.DirectorySeparatorChar.ToString())}]*";


    }
}
