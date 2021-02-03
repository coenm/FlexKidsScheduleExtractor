namespace Reporter.GoogleCalendar
{
    public class GoogleCalendarConfig
    {
        public GoogleCalendarConfig(
            string googleCalendarAccount,
            string googleCalendarId,
            string pkcs8PrivateKey)
        {
            GoogleCalendarAccount = googleCalendarAccount;
            GoogleCalendarId = googleCalendarId;
            Pkcs8PrivateKey = pkcs8PrivateKey;
        }

        public string GoogleCalendarAccount { get; }

        public string GoogleCalendarId { get; }

        public string Pkcs8PrivateKey { get; }
    }
}