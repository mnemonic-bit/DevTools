using FindProjSln.Configuration;
using FindProjSln.FileSystem;
using System.Xml.Linq;

namespace FindProjSln
{
    internal class ProjectFinder
    {

        public ProjectFinder(
            AppConfig config,
            SolutionFinder solutionFinder)
        {
            _config = config;
            _solutionFinder = solutionFinder;
        }

        public void Start()
        {
            if(string.IsNullOrEmpty(_config.ProjectFilePath))
            {
                FindProjectsAndSolutions();
            }
            else
            {
                FindSolutionForProjectFile(_config.ProjectFilePath);
            }
        }


        private const string CSPROJ_FILE_EXTENSION = ".csproj";
        private const string VCXPROJ_FILE_EXTENSION = ".vcxproj";
        private static readonly string[] ProjectFileExtensions = [CSPROJ_FILE_EXTENSION, VCXPROJ_FILE_EXTENSION];
        private readonly AppConfig _config;
        private readonly SolutionFinder _solutionFinder;


        private void FindProjectsAndSolutions()
        {
            var owningProjects = FindOwningProjects(
                _config.FileNames.First(),
                _config.Path,
                _config.TraverseRoot);

            foreach (var project in owningProjects)
            {
                Console.WriteLine(project);
                if (_config.FindSolutions)
                {
                    foreach (var sln in _solutionFinder.FindSolutions(project))
                    {
                        Console.WriteLine($" --IN--> {sln}");
                    }
                }
            }
        }

        private void FindSolutionForProjectFile(string projectFilePath)
        {
            foreach (var sln in _solutionFinder.FindSolutions(projectFilePath))
            {
                Console.WriteLine($" --IN--> {sln}");
            }
        }


        /// <summary>
        /// Returns all project files that claim ownership of the given source file.
        /// Walks up the directory tree and checks each candidate project.
        /// </summary>
        private IEnumerable<string> FindOwningProjects(string sourcePath, string? rootPath = null, bool traverseRoot = false)
        {
            var path = Path.GetDirectoryName(sourcePath);

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"The given path is not valid. Cannot determine the parent directory of the path: {sourcePath}");
            }

            return ProjectFilesAlongPath(path, rootPath, traverseRoot)
                .Where(proj => ProjectOwnsFile(proj, sourcePath));
        }

        /// <summary>
        /// Lists all project files that are placed along the path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="rootPath">The root directory, which represents the boundaries for the traversal.</param>
        /// <param name="traverseWholeTree">If true, the algorithm traverses all subfolder of the root directory.</param>
        /// <returns></returns>
        private IEnumerable<string> ProjectFilesAlongPath(string path, string? rootPath = null, bool traverseWholeTree = false)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var directoriesToTraverse = path.EnumerateDirectoriesAlongPath();
            if (traverseWholeTree)
            {
                if (string.IsNullOrEmpty(rootPath))
                {
                    throw new ArgumentException($"The parameter {nameof(rootPath)} must not be null or empty if {nameof(traverseWholeTree)} is set true.");
                }
                directoriesToTraverse = rootPath.EnumerateAllSubDirectories();
            }

            return directoriesToTraverse
                .SelectMany(dir => ProjectFileExtensions
                    .Select(fileExtension => $"*{fileExtension}")
                    .SelectMany(fileExtension => Directory.EnumerateFiles(dir, fileExtension))
                    .Select(p => p.NormalizeSeparatorChars())
                    .Where(projFile => visited.Add(projFile)));
        }

        #region Project-Membership Checks

        private static bool ProjectOwnsFile(string projectPath, string sourcePath)
        {
            try
            {
                string projectDir = Path.GetDirectoryName(projectPath)!;

                // The source file must be at or below the project directory.
                if (!sourcePath.IsUnderDirectory(projectDir))
                {
                    return false;
                }

                var projDoc = XDocument.Load(projectPath);
                string extension = Path.GetExtension(projectPath).ToLowerInvariant();

                return extension switch
                {
                    CSPROJ_FILE_EXTENSION => CsprojOwnsFile(projDoc, projectDir, sourcePath),
                    VCXPROJ_FILE_EXTENSION => VcxprojOwnsFile(projDoc, projectDir, sourcePath),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Warning: could not parse {projectPath}: {ex.Message}");
                return false;
            }
        }

        private static bool CsprojOwnsFile(XDocument doc, string projectDir, string sourcePath)
        {
            // Detect SDK-style project: has a Sdk attribute/element or no <ItemGroup> with explicit Compile items.
            bool isSdkStyle = IsSdkStyleProject(doc);

            if (isSdkStyle)
            {
                return SdkStyleCsprojOwnsFile(doc, projectDir, sourcePath);
            }
            else
            {
                return LegacyCsprojOwnsFile(doc, projectDir, sourcePath);
            }
        }

        /// <summary>
        /// SDK-style projects include all files under the project directory by default
        /// unless explicitly excluded. We mirror that logic here.
        /// </summary>
        private static bool SdkStyleCsprojOwnsFile(XDocument doc, string projectDir, string sourcePath)
        {
            // Default included extensions for C# SDK projects
            string[] defaultIncludes = ["*.cs"];
            string sourceExt = Path.GetExtension(sourcePath).ToLowerInvariant();
            string relPath = Path.GetRelativePath(projectDir, sourcePath);

            bool includedByDefault = defaultIncludes.Any(p => string.Equals(sourceExt, Path.GetExtension(p), StringComparison.OrdinalIgnoreCase));

            if (!includedByDefault)
            {
                return HasExplicitInclude(doc, projectDir, sourcePath, "Compile", "Content", "None", "EmbeddedResource");
            }

            bool isExplicitlyExcluded = IsExplicitlyRemoved(doc, projectDir, sourcePath);
            bool isExplicitlyIncluded = HasExplicitInclude(doc, projectDir, sourcePath, "Compile");

            return isExplicitlyIncluded || !isExplicitlyExcluded && sourcePath.IsUnderDirectory(projectDir);
        }

        private static bool LegacyCsprojOwnsFile(XDocument doc, string projectDir, string sourcePath)
        {
            return HasExplicitInclude(doc, projectDir, sourcePath, "Compile", "Content", "None", "EmbeddedResource", "ClCompile", "ClInclude");
        }

        private static bool VcxprojOwnsFile(XDocument doc, string projectDir, string sourcePath)
        {
            return HasExplicitInclude(doc, projectDir, sourcePath, "ClCompile", "ClInclude", "None", "ResourceCompile", "CustomBuild");
        }

        #endregion

        #region XML Helpers

        /// <summary>
        /// Checks whether any MSBuild item element with one of the given item types
        /// has an Include/Update attribute that resolves to the source file,
        /// supporting wildcards (e.g., **\*.cs).
        /// </summary>
        static bool HasExplicitInclude(XDocument doc, string projectDir, string sourcePath, params string[] itemTypes)
        {
            var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
            var itemTypeSet = new HashSet<string>(itemTypes, StringComparer.OrdinalIgnoreCase);

            foreach (var item in doc.Descendants())
            {
                if (!itemTypeSet.Contains(item.Name.LocalName))
                {
                    continue;
                }

                var include = item.Attribute("Include")?.Value ?? item.Attribute("Update")?.Value;
                if (include is null)
                {
                    continue;
                }

                if (projectDir.GlobMatches(include, sourcePath))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsExplicitlyRemoved(XDocument doc, string projectDir, string sourcePath)
        {
            var ns = doc.Root?.Name.Namespace ?? XNamespace.None;

            foreach (var item in doc.Descendants())
            {
                if (!string.Equals(item.Name.LocalName, "Compile", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var remove = item.Attribute("Remove")?.Value;
                if (remove is not null && projectDir.GlobMatches(remove, sourcePath))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsSdkStyleProject(XDocument doc)
        {
            var root = doc.Root;
            if (root is null)
            {
                return false;
            }

            // <Project Sdk="Microsoft.NET.Sdk"> or similar
            if (root.Attribute("Sdk") is not null)
            {
                return true;
            }

            // <Sdk Name="Microsoft.NET.Sdk" /> child element
            if (root.Elements().Any(e => string.Equals(e.Name.LocalName, "Sdk", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Absence of a <ToolsVersion> attribute is a weak signal but not reliable alone.
            return false;
        }

        #endregion

    }
}
