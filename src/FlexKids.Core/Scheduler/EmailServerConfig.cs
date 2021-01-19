namespace FlexKids.Core.Scheduler
{
    public class EmailServerConfig
    {
        public EmailServerConfig(string smtpHost, int smtpPort, string smtpUsername, string smtpPassword, bool sslTls)
        {
            SmtpHost = smtpHost;
            SmtpPort = smtpPort;
            SmtpUsername = smtpUsername;
            SmtpPassword = smtpPassword;
            SslTls = sslTls;
        }

        public string SmtpHost { get; }

        public int SmtpPort { get; }

        public bool SslTls { get; }

        public string SmtpUsername { get; }

        public string SmtpPassword { get; }
    }
}