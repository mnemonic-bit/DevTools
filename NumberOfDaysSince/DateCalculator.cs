using NumberOfDaysSince.Configuration;

namespace NumberOfDaysSince
{
    internal class DateCalculator
    {

        public DateCalculator(AppConfig config)
        {
            _config = config;
        }

        public DateTime ConvertNumberOfDaysToDate(int numberOfDays, DateTime? referenceDate)
        {
            var actualReferenceDate = referenceDate ?? new DateTime(1, 1, 1);

            return actualReferenceDate.AddDays(numberOfDays);
        }

        public int GetNumberOfDaysSince(DateTime date, DateTime? referenceDate)
        {
            var actualReferenceDate = referenceDate ?? new DateTime(1, 1, 1);

            return date.Subtract(actualReferenceDate).Days;
        }


        private readonly AppConfig _config;


    }
}
