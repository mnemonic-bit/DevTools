using CommandLine;

namespace FindProjSln.Configuration
{
    internal class AppConfig
    {

        [Value(0, MetaName = "filenames", HelpText = "Source code file paths for which this tool will try to find the project file.")]
        public IEnumerable<string> FileNames { get; set; } = new List<string>();

        [Option('p', "path", Required = false, Default = ".", HelpText = "The base path of the code base.")]
        public string Path { get; set; } = ".";

        [Option('t', "traverse", Default = false, HelpText = "If true, traverses all sub-directories from the given root path. Note that with this parameter set, the path-parameter must also be set.")]
        public bool TraverseRoot { get; set; }

    }
}
