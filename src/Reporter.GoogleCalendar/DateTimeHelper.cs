namespace Reporter.GoogleCalendar
{
    using System;
    using System.Globalization;

    internal static class DateTimeHelper
    {
        /// <summary>
        /// Calculate DateTime for the Monday for the given year and week.
        /// </summary>
        /// <param name="year">year. I.e. <c>2020</c>.</param>
        /// <param name="weekNumber">integer between <c>1</c> and <c>53</c> representing the week number.</param>
        /// <remarks>Implementation taken from <see href="http://stackoverflow.com/questions/662379/calculate-date-from-week-number">stackoverflow</see>.</remarks>
        /// <returns>Returns the DateTime for the Monday for the given year and week.</returns>
        public static DateTime GetMondayForGivenWeek(int year, int weekNumber)
        {
            // Assume January 1 of year xx is week 1
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);

            Calendar calendar = CultureInfo.CurrentCulture.Calendar;
            var firstWeek = calendar.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekNumber;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            DateTime result = firstThursday.AddDays(weekNum * 7);
            DateTime mondayOfRequestedWeek = result.AddDays(-3);

            return mondayOfRequestedWeek;
        }
    }
}