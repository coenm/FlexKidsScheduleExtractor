namespace FlexKids.Core.Scheduler
{
    using System;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;

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

        public async Task Send(MailMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await _client.SendMailAsync(message);
        }
    }
}
