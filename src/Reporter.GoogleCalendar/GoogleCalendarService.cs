namespace Reporter.GoogleCalendar
{
    using System;
    using System.Threading.Tasks;
    using Google.Apis.Calendar.v3;
    using Google.Apis.Calendar.v3.Data;

    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly CalendarService _service;

        public GoogleCalendarService(CalendarService service)
        {
            _service = service;
        }

        public Task<Calendar> GetCalendarById(string id)
        {
            return _service.Calendars.Get(id).ExecuteAsync();
        }

        public EventsResource.ListRequest CreateListRequest(string calendarId)
        {
            return _service.Events.List(calendarId);
        }

        public EventsResource.ListRequest CreateListRequestForWeek(string calendarId, Repository.Model.Week week)
        {
            DateTime mondayOfRequestedWeek = DateTimeHelper.GetMondayForGivenWeek(week.Year, week.WeekNr);

            var request = _service.Events.List(calendarId);
            request.ShowDeleted = true;
            request.ShowDeleted = false;
            request.MaxResults = 100;
            request.SharedExtendedProperty = "Week=" + week.Year + "-" + week.WeekNr;

            DateTime start = mondayOfRequestedWeek.AddDays(-7); // one week before
            request.TimeMin = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0);

            DateTime end = mondayOfRequestedWeek.AddDays(4).AddDays(7); // fridays one week after
            request.TimeMax = new DateTime(end.Year, end.Month, end.Day, 0, 0, 0);

            return request;
        }

        public Task<Events> GetEvents(EventsResource.ListRequest listRequest)
        {
            return listRequest.ExecuteAsync();
        }

        public Task<string> DeleteEvent(string calendarId, Event calendarEvent)
        {
            var deleteRequest = _service.Events.Delete(calendarId, calendarEvent.Id);
            return deleteRequest.ExecuteAsync();
        }

        public Task<Event> InsertEvent(string calendarId, Event calendarEvent)
        {
            return _service.Events.Insert(calendarEvent, calendarId).ExecuteAsync();
        }

        public void Dispose()
        {
            // ignore
        }
    }
}
