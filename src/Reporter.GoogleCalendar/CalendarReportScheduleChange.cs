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
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IFlexKidsConfig _flexKidsConfig;

        public CalendarReportScheduleChange(IDateTimeProvider dateTimeProvider, IFlexKidsConfig flexKidsConfig)
        {
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _flexKidsConfig = flexKidsConfig ?? throw new ArgumentNullException(nameof(flexKidsConfig));
        }

        public bool HandleChange(IList<FlexKidsScheduler.Model.ScheduleDiff> schedule)
        {
            if (schedule == null || !schedule.Any())
            {
                _logger.Trace("HandleChange Google calender, schedule == null | count = 0");
                return true;
            }

            try
            {
                _logger.Trace("Create Google Calendar");
                var google = new GoogleCalendarScheduler(_flexKidsConfig);
                _logger.Trace("Make events");
                google.MakeEvents(schedule);
            }
            catch (Exception ex)
            {
                _logger.Error("Something went wrong using Google Calendar.", ex);
                return false;
            }

            _logger.Trace("Done Google calendar");
            return true;
        }
    }
}
