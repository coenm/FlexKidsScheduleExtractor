namespace FlexKids.Console
{
    using FlexKidsScheduler;

    public class StaticFlexKidsConfig : IFlexKidsConfig
    {
        public StaticFlexKidsConfig(string emailFrom, string emailTo2, string emailToName2, string emailTo1, string emailToName1, string smtpHost, int smtpPort, string smtpUsername, string smtpPassword, string googleCalendarAccount, string googleCalendarId, byte[] googleCalendarKey, bool sslTls)
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
            GoogleCalendarKey = googleCalendarKey;
            SslTls = sslTls;
        }

        public string EmailFrom { get; }

        public string EmailTo2 { get; }

        public string EmailToName2 { get; }

        public string EmailTo1 { get; }

        public string EmailToName1 { get; }

        public string SmtpHost { get; }

        public int SmtpPort { get; }

        public bool SslTls { get; }

        public string SmtpUsername { get; }

        public string SmtpPassword { get; }

        public string GoogleCalendarAccount { get; }

        public string GoogleCalendarId { get; }

        public byte[] GoogleCalendarKey { get; }
    }
}