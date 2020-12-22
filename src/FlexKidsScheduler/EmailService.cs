namespace FlexKidsScheduler
{
    using System;
    using System.Net;
    using System.Net.Mail;

    public class EmailService : IEmailService
    {
        private readonly SmtpClient _client;

        public EmailService(IFlexKidsConfig flexKidsConfig)
        {
            _client = new SmtpClient
                {
                    Port = flexKidsConfig.SmtpPort,
                    Host = flexKidsConfig.SmtpHost,
                    EnableSsl = false,
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
                throw new ArgumentNullException("message");
            }

            _client.Send(message); // TODO exception handling
        }
    }
}
