namespace FlexKidsScheduler
{
    using System.Net.Mail;

    public interface IEmailService
    {
        void Send(MailMessage message);
    }
}
