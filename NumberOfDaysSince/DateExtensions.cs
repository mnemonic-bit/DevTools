using System.Globalization;

namespace NumberOfDaysSince
{
    public static class DateExtensions
    {

        public static bool TryParseAnyFormat(this string dateString, out DateTime dateTime)
        {
            dateTime = default;

            if (string.IsNullOrWhiteSpace(dateString))
            {
                return false;
            }

            if (DateTime.TryParse(dateString, CultureInfo.CurrentCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out dateTime))
            {
                return true;
            }

            if (DateTime.TryParseExact(
                dateString,
                KnownDateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                out dateTime))
            {
                return true;
            }

            return false;
        }


        private static readonly string[] KnownDateFormats = {
                // ISO 8601 Formats
                "yyyy-MM-ddTHH:mm:ss.fffffffZ", "yyyy-MM-ddTHH:mm:ssZ",
                "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd",
            
                // US Formats (Month first)
                "MM/dd/yyyy", "MM/dd/yyyy HH:mm:ss", "M/d/yyyy", "M/d/yyyy h:mm:ss tt",
            
                // European/UK Formats (Day first)
                "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss", "d/M/yyyy", "d/M/yyyy H:mm:ss",

                // Dotted notation
                "dd.MM.yyyy", "dd.MM.yyyy HH:mm:ss", "d.M.yyyy", "d.M.yyyy H:mm:ss",
            
                // Other common variations
                "yyyy/MM/dd", "yyyyMMdd", "dd-MMM-yyyy", "MMM dd, yyyy"
            };


    }
}
