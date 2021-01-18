namespace FlexKidsScheduler
{
    using System.Net.Mail;
    using System.Threading.Tasks;

    public interface IEmailService
    {
        Task Send(MailMessage message);
    }
}
