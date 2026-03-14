using FindProjSln.Configuration;
using FindProjSln.FileSystem;
using System.Text.RegularExpressions;

namespace FindProjSln
{
    internal class SolutionFinder
    {

        public SolutionFinder(AppConfig config)
        {
            _config = config;
        }

        internal IEnumerable<string> FindSolutions(string projectFilePath)
        {
            return FindSolutions(
                projectFilePath,
                _config.Path,
                _config.TraverseRoot);
        }


        private static readonly Regex ProjectLineRegex = new(
            @"^\s*Project\(""{[^}]+}""\)\s*=\s*""[^""]*""\s*,\s*""(?<path>[^""]+)""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly AppConfig _config;


        private IEnumerable<string> FindSolutions(string projectPath, string? rootPath = null, bool traverseWholeTree = false)
        {
            return SolutionFilesAlongThePath(projectPath, rootPath, traverseWholeTree)
                .Where(slnPath => SolutionReferencesProject(slnPath, projectPath));
        }

        private IEnumerable<string> SolutionFilesAlongThePath(string projectFilePath, string? rootPath = null, bool traverseWholeTree = false)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var projectDirectoryPath = Path.GetDirectoryName(projectFilePath) ?? throw new Exception($"Cannot retreive the path from the project file: {projectFilePath}");
            var directoriesToTraverse = projectDirectoryPath.EnumerateDirectoriesAlongPath();
            if (traverseWholeTree)
            {
                if (string.IsNullOrEmpty(rootPath))
                {
                    throw new ArgumentException($"The parameter {nameof(rootPath)} must not be null or empty if {nameof(traverseWholeTree)} is set true.");
                }
                directoriesToTraverse = rootPath.EnumerateAllSubDirectories();
            }

            return directoriesToTraverse
                .SelectMany(dir => Directory.EnumerateFiles(dir, "*.sln"))
                .Where(slnFile => visited.Add(slnFile));
        }

        private bool SolutionReferencesProject(string slnPath, string fullProjectPath)
        {
            string slnDir = Path.GetDirectoryName(slnPath)!;

            foreach (string line in File.ReadLines(slnPath))
            {
                var match = ProjectLineRegex.Match(line);
                if (!match.Success)
                {
                    continue;
                }

                string rawPath = match.Groups["path"].Value.NormalizeSeparatorChars();
                string resolvedPath = Path.GetFullPath(Path.Combine(slnDir, rawPath));

                if (string.Equals(resolvedPath, fullProjectPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


    }
}
