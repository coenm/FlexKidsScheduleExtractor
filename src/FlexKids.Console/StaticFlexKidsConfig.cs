namespace FlexKids.Console
{
    using FlexKidsScheduler;

    public class StaticFlexKidsConfig : IFlexKidsConfig
    {
        public StaticFlexKidsConfig(string emailFrom, string emailTo2, string emailToName2, string emailTo1, string emailToName1, string smtpHost, int smtpPort, string smtpUsername, string smtpPassword, string googleCalendarAccount, string googleCalendarId, string googleCalendarKeyFile)
        {
            EmailFrom = emailFrom;
            EmailTo2 = emailTo2;
            EmailToName2 = emailToName2;
            EmailTo1 = emailTo1;
            EmailToName1 = emailToName1;
            SmtpHost = smtpHost;
            SmtpPort = smtpPort;
            SmtpUsername = smtpUsername;
            SmtpPassword = smtpPassword;
            GoogleCalendarAccount = googleCalendarAccount;
            GoogleCalendarId = googleCalendarId;
            GoogleCalendarKeyFile = googleCalendarKeyFile;
        }

        public string EmailFrom { get; }

        public string EmailTo2 { get; }

        public string EmailToName2 { get; }

        public string EmailTo1 { get; }

        public string EmailToName1 { get; }

        public string SmtpHost { get; }

        public int SmtpPort { get; }

        public string SmtpUsername { get; }

        public string SmtpPassword { get; }

        public string GoogleCalendarAccount { get; }

        public string GoogleCalendarId { get; }

        public string GoogleCalendarKeyFile { get; }
    }
}