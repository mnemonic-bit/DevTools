using CommandLine;

namespace NumberOfDaysSince.Configuration
{
    internal class AppConfig
    {

        [Value(0, MetaName = "date", Required = false, HelpText = "The date which is used to calculate the number of days to.")]
        public string Date { get; set; } = "";

        [Option('r', "reference-date", Required = false, Default = "1.1.0001", HelpText = "The reference date.")]
        public string ReferenceDate { get; set; } = "1.1.0001";

        [Option('n', "number-of-days", Default = null, HelpText = "The number of days which is added to the reference date.")]
        public int? NumberOfDays { get; set; }

    }
}
