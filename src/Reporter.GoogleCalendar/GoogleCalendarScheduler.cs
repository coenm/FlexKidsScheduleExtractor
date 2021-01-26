namespace Reporter.GoogleCalendar
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using FlexKids.Core.Repository.Model;
    using FlexKids.Core.Scheduler.Model;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Calendar.v3;
    using Google.Apis.Calendar.v3.Data;
    using Google.Apis.Services;
    using Calendar = Google.Apis.Calendar.v3.Data.Calendar;

    internal class GoogleCalendarScheduler : IDisposable
    {
        private readonly IGoogleCalendarService _calendarService;
        private readonly string _googleCalendarId;

        public GoogleCalendarScheduler(GoogleCalendarConfig config)
        {
            _ = config ?? throw new ArgumentNullException(nameof(config));
            _googleCalendarId = config.GoogleCalendarId;

            var certificate = new X509Certificate2(config.GoogleCalendarKey);

            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(config.GoogleCalendarAccount)
                    {
                        Scopes = new[] { CalendarService.Scope.Calendar, },
                    }.FromCertificate(certificate));

            // Create the service.
            var service = new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientFactory = new Google.Apis.Http.HttpClientFactory(),
                    HttpClientInitializer = credential,
                    ApplicationName = "FlexKids Rooster by CoenM",
                });

            _calendarService = new GoogleCalendarService(service);
        }

        public async Task MakeEvents(IReadOnlyList<ScheduleDiff> schedule, WeekSchedule updatedWeekSchedule)
        {
            Calendar calendar = await _calendarService.GetCalendarById(_googleCalendarId);
            if (calendar == null)
            {
                throw new CalendarNotFoundException(_googleCalendarId);
            }

            EventsResource.ListRequest request = _calendarService.CreateListRequestForWeek(_googleCalendarId, updatedWeekSchedule.Year, updatedWeekSchedule.WeekNumber);

            Events result = await _calendarService.GetEvents(request);
            var allRows = new List<Event>();
            while (result.Items != null)
            {
                // Add the rows to the final list
                allRows.AddRange(result.Items);

                // We will know we are on the last page when the next page token is
                // null.
                // If this is the case, break.
                if (result.NextPageToken == null)
                {
                    break;
                }

                // Prepare the next page of results
                request.PageToken = result.NextPageToken;

                // Execute and process the next page request
                result = await _calendarService.GetEvents(request);
            }

            foreach (Event ev in allRows)
            {
                _ = await _calendarService.DeleteEvent(_googleCalendarId, ev);
            }

            // add items to calendar.
            IOrderedEnumerable<ScheduleDiff> addedSchedules = schedule
                                                              .Where(x => x.Status is ScheduleStatus.Added or ScheduleStatus.Unchanged)
                                                              .OrderBy(x => x.Start)
                                                              .ThenBy(x => x.Status);

            foreach (ScheduleDiff item in addedSchedules)
            {
                var extendedProperty = new Event.ExtendedPropertiesData
                    {
                        Shared = new Dictionary<string, string>
                            {
                                { "Week", item.SingleShift.WeekSchedule.Year + "-" + item.SingleShift.WeekSchedule.WeekNumber },
                            },
                    };

                // queryEvent.SharedExtendedProperty = "EventID=3684";
                var newEvent = new Event
                    {
                        Start = new EventDateTime
                            {
                                DateTime = item.SingleShift.StartDateTime,
                            },
                        End = new EventDateTime
                            {
                                DateTime = item.SingleShift.EndDateTime,
                            },
                        Description = $"it is time to work {DateTime.Now.ToString(CultureInfo.InvariantCulture)}" ,
                        Location = item.SingleShift.Location,
                        Summary = item.SingleShift.Location,
                        ExtendedProperties = extendedProperty,
                    };

                _ = await _calendarService.InsertEvent(_googleCalendarId, newEvent);
            }
        }

        public void Dispose()
        {
            // do nothing
        }
    }
}