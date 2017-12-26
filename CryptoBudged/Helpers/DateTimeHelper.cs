using System;

namespace CryptoBudged.Helpers
{
    public class DateTimeHelper
    {
        public static DateTime DateAndTimeToDateTime(DateTime? date, DateTime? time)
        {
            var value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);

            if (date != null)
            {
                value = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day);
            }
            if (time != null)
            {
                value = new DateTime(value.Year, value.Month, value.Day, time.Value.Hour, time.Value.Minute, time.Value.Second);
            }

            return value;
        }
    }
}
