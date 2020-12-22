namespace FlexKidsScheduler
{
    public interface IFlexKidsConfig
    {
        string EmailFrom { get; }

        string EmailTo2 { get; }

        string EmailToName2 { get; }

        string EmailTo1 { get; }

        string EmailToName1 { get; }

        string SmtpHost { get; }

        int SmtpPort { get; }

        string SmtpUsername { get; }

        string SmtpPassword { get; }

        string GoogleCalendarAccount { get; }

        string GoogleCalendarId { get; }

        string GoogleCalendarKeyFile { get; }
    }
}
