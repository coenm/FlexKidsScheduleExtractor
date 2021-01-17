namespace FlexKidsScheduler
{
    using System;
    using System.Net;
    using System.Net.Mail;

    public class EmailService : IEmailService
    {
        private readonly SmtpClient _client;

        public EmailService(EmailServerConfig flexKidsConfig)
        {
            if (flexKidsConfig == null)
            {
                throw new ArgumentNullException(nameof(flexKidsConfig));
            }

            _client = new SmtpClient
                {
                    Port = flexKidsConfig.SmtpPort,
                    Host = flexKidsConfig.SmtpHost,
                    EnableSsl = flexKidsConfig.SslTls,
                    Timeout = 10000,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(flexKidsConfig.SmtpUsername, flexKidsConfig.SmtpPassword),
                };
        }

        public void Send(MailMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            // todo async await
            _client.Send(message); // TODO exception handling
        }
    }
}
