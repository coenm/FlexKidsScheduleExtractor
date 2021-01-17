namespace Reporter.GoogleCalendar
{
    public class GoogleCalendarConfig
    {
        public GoogleCalendarConfig(string googleCalendarAccount, string googleCalendarId, byte[] googleCalendarKey)
        {
            GoogleCalendarAccount = googleCalendarAccount;
            GoogleCalendarId = googleCalendarId;
            GoogleCalendarKey = googleCalendarKey;
        }

        public string GoogleCalendarAccount { get; }

        public string GoogleCalendarId { get; }

        public byte[] GoogleCalendarKey { get; }
    }
}