namespace Reporter.GoogleCalendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FlexKidsScheduler;
    using NLog;

    public class CalendarReportScheduleChange : IReportScheduleChange
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly GoogleCalendarConfig _config;

        public CalendarReportScheduleChange(GoogleCalendarConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public bool HandleChange(IReadOnlyList<FlexKidsScheduler.Model.ScheduleDiff> schedule)
        {
            if (schedule == null || !schedule.Any())
            {
                _logger.Trace("HandleChange Google calender, schedule == null | count = 0");
                return true;
            }

            try
            {
                _logger.Trace("Create Google Calendar");
                var google = new GoogleCalendarScheduler(_config);
                _logger.Trace("Make events");
                google.MakeEvents(schedule);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Something went wrong using Google Calendar.");
                return false;
            }

            _logger.Trace("Done Google calendar");
            return true;
        }
    }
}
