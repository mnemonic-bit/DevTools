using NumberOfDaysSince.Configuration;

namespace NumberOfDaysSince
{
    internal class App
    {

        public App(
            AppConfig config,
            DateCalculator dateCalculator)
        {
            _config = config;
            _dateCalculator = dateCalculator;
        }

        public void Start()
        {
            if (_config.NumberOfDays.HasValue)
            {
                CalculateDateFromNumberOfDays(
                    _config.NumberOfDays.Value,
                    _config.ReferenceDate);
            }
            else
            {
                CalculateNumberOfDays(
                    _config.Date,
                    _config.ReferenceDate);
            }
        }


        private readonly AppConfig _config;
        private readonly DateCalculator _dateCalculator;


        private void CalculateNumberOfDays(string dateString, string referenceDateString)
        {
            if (!dateString.TryParseAnyFormat(out var date))
            {
                throw new ArgumentException($"The given date '{dateString}' cannot be converted.");
            }

            if (!referenceDateString.TryParseAnyFormat(out var referenceDate))
            {
                throw new ArgumentException($"The given date '{referenceDateString}' cannot be converted.");
            }

            Console.WriteLine($"{_dateCalculator.GetNumberOfDaysSince(date, referenceDate)}");
        }

        private void CalculateDateFromNumberOfDays(int numberOfDays, string referenceDateString)
        {
            if (!referenceDateString.TryParseAnyFormat(out var referenceDate))
            {
                throw new ArgumentException($"The given date '{referenceDateString}' cannot be converted.");
            }

            Console.WriteLine($"{_dateCalculator.ConvertNumberOfDaysToDate(numberOfDays, referenceDate)}");
        }

    }
}
