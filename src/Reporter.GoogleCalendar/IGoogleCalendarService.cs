namespace Reporter.GoogleCalendar
{
    using System;
    using System.Threading.Tasks;
    using Google.Apis.Calendar.v3;
    using Google.Apis.Calendar.v3.Data;

    public interface IGoogleCalendarService : IDisposable
    {
        Task<Calendar> GetCalendarById(string id);

        EventsResource.ListRequest CreateListRequestForWeek(string calendarId, int year, int weekNumber);

        Task<Events> GetEvents(EventsResource.ListRequest listRequest);

        Task<string> DeleteEvent(string calendarId, Event calendarEvent);

        Task<Event> InsertEvent(string calendarId, Event calendarEvent);
    }
}
