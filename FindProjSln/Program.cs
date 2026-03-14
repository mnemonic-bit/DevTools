using CommandLine;
using FindProjSln.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FindProjSln
{
    /// <summary>
    /// This tool helps finding the csproj or vcsproj file a given
    /// source file is included in.
    /// </summary>
    public class Program
    {

        public static int Main(params string[] args)
        {
            return Parser.Default
                .ParseArguments<AppConfig>(args)
                .MapResult(LaunchApplication, HandleParseError);
        }


        private IServiceProvider _serviceProvider;


        private Program(AppConfig appConfig)
        {
            var services = new ServiceCollection()
                .AddSingleton(LoadConfiguration(appConfig))
                .AddSingleton(new CancellationTokenSource())
                .AddScoped<ProjectFinder>();

            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Handles the parse errors of the CommandLineParse library for all
        /// the command line arguments that have been passed to this app.
        /// </summary>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static int HandleParseError(IEnumerable<Error> errors)
        {
            return -1;
        }

        /// <summary>
        /// Launches this application. It will initialize an instance of this
        /// <see cref="Program"/> and act upon the passed argumetns.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static int LaunchApplication(AppConfig config)
        {
            if (!ValidateConfiguration(config))
            {
                return -1;
            }

            var app = new Program(config);
            app.Start();

            return 0;
        }

        private AppConfig LoadConfiguration(AppConfig appConfig)
        {
            // We can load additional configuration values for this app
            // at this point and inject them into the AppConfig instance.

            return appConfig;
        }

        private void Start()
        {
            _serviceProvider
                .GetRequiredService<ProjectFinder>()
                .Start();
        }

        private static bool ValidateConfiguration(AppConfig appConfig)
        {
            // Perform checks of the configuration.

            return true;
        }

    }
}