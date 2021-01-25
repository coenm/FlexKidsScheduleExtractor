namespace Reporter.GoogleCalendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FlexKids.Core.Interfaces;
    using FlexKids.Core.Repository.Model;
    using FlexKids.Core.Scheduler.Model;
    using Microsoft.Extensions.Logging;

    public class CalendarReportScheduleChange : IReportScheduleChange
    {
        private readonly ILogger _logger;
        private readonly GoogleCalendarConfig _config;

        public CalendarReportScheduleChange(ILogger logger, GoogleCalendarConfig config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<bool> HandleChange(IReadOnlyList<ScheduleDiff> schedule, WeekSchedule updatedWeekSchedule)
        {
            if (schedule == null || !schedule.Any())
            {
                _logger.LogTrace("HandleChange Google calender, schedule == null | count = 0");
                return true;
            }

            try
            {
                _logger.LogTrace("Create Google Calendar");
                var google = new GoogleCalendarScheduler(_config);
                _logger.LogTrace("Make events");
                await google.MakeEvents(schedule, updatedWeekSchedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong using Google Calendar.");
                return false;
            }

            _logger.LogTrace("Done Google calendar");
            return true;
        }
    }
}
