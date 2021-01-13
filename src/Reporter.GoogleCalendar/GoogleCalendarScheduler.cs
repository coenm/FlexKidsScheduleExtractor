namespace Reporter.GoogleCalendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using FlexKidsScheduler;
    using FlexKidsScheduler.Model;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Calendar.v3;
    using Google.Apis.Calendar.v3.Data;
    using Google.Apis.Services;
    using Repository.Model;

    internal class GoogleCalendarScheduler : IDisposable
    {
        private readonly IGoogleCalendarService _calendarService;
        private readonly string _googleCalendarId;

        public GoogleCalendarScheduler(IFlexKidsConfig flexKidsConfig)
        {
            _ = flexKidsConfig ?? throw new ArgumentNullException(nameof(flexKidsConfig));
            _googleCalendarId = flexKidsConfig.GoogleCalendarId;

            var certificate = new X509Certificate2(flexKidsConfig.GoogleCalendarKey);

            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(flexKidsConfig.GoogleCalendarAccount)
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

            Calendar calendar = _calendarService.GetCalendarById(_googleCalendarId);
            if (calendar == null)
            {
                throw new CalendarNotFoundException(_googleCalendarId);
            }
        }

        public void MakeEvents(IReadOnlyList<ScheduleDiff> schedule)
        {
            var weeks = schedule.Select(x => x.Schedule.Week).Distinct();

            foreach (Week week in weeks)
            {
                var request = _calendarService.CreateListRequestForWeek(_googleCalendarId, week);

                var result = _calendarService.GetEvents(request);
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
                    result = _calendarService.GetEvents(request);
                }

                foreach (Event ev in allRows)
                {
                    _ = _calendarService.DeleteEvent(_googleCalendarId, ev);
                }
            }

            // add items to calendar.
            foreach (var item in schedule.Where(x => x.Status == ScheduleStatus.Added).OrderBy(x => x.Start).ThenBy(x => x.Status))
            {
                var extendedProperty = new Event.ExtendedPropertiesData
                    {
                        Shared = new Dictionary<string, string>
                            {
                                { "Week", item.Schedule.Week.Year + "-" + item.Schedule.Week.WeekNr },
                            },
                    };

                // queryEvent.SharedExtendedProperty = "EventID=3684";
                var newEvent = new Event
                    {
                        Start = new EventDateTime
                            {
                                DateTime = item.Schedule.StartDateTime,
                            },
                        End = new EventDateTime
                            {
                                DateTime = item.Schedule.EndDateTime,
                            },
                        Description = "it is time to work",
                        Location = item.Schedule.Location,
                        Summary = item.Schedule.Location,
                        ExtendedProperties = extendedProperty,
                    };

                _ = _calendarService.InsertEvent(_googleCalendarId, newEvent);
            }
        }

        public void Dispose()
        {
            // do nothing
        }
    }
}