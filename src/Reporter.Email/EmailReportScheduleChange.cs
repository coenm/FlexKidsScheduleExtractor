namespace Reporter.Email
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;
    using FlexKids.Core.Interfaces;
    using FlexKids.Core.Repository.Model;
    using FlexKids.Core.Scheduler;
    using FlexKids.Core.Scheduler.Model;
    using Microsoft.Extensions.Logging;

    public class EmailReportScheduleChange : IReportScheduleChange
    {
        private readonly EmailConfig _emailConfig;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;

        public EmailReportScheduleChange(
            EmailConfig flexKidsConfig,
            IEmailService emailService,
            ILogger logger)
        {
            _emailConfig = flexKidsConfig ?? throw new ArgumentNullException(nameof(flexKidsConfig));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> HandleChange(IReadOnlyList<ScheduleDiff> schedule, WeekSchedule updatedWeekSchedule)
        {
            if (schedule == null || !schedule.Any())
            {
                return true;
            }

            try
            {
                ScheduleDiff[] orderedSchedule = schedule.OrderBy(x => x.Start).ThenBy(x => x.Status).ToArray();
                var subject = "Werkrooster voor week " + updatedWeekSchedule.WeekNumber;
                var schedulePlain = EmailContentBuilder.ScheduleToPlainTextString(orderedSchedule);
                var scheduleHtml = EmailContentBuilder.ScheduleToHtmlString(orderedSchedule);

                MailMessage mailMessage = CreateMailMessage(subject, schedulePlain, scheduleHtml);
                await _emailService.Send(mailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong sending an email with the schedule.");
                return false;
            }

            return true;
        }

        private MailMessage CreateMailMessage(string subject, string schedulePlain, string scheduleHtml)
        {
            var mailMessage = new MailMessage(_emailConfig.EmailFrom, _emailConfig.To.First())
                {
                    Subject = subject,
                    Body = schedulePlain,
                    BodyEncoding = Encoding.UTF8,
                    DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure,
                };

            foreach (MailAddress address in _emailConfig.To.Skip(1))
            {
                mailMessage.To.Add(address);
            }

            var mimeType = new ContentType("text/html");
            var alternate = AlternateView.CreateAlternateViewFromString(scheduleHtml, mimeType);
            mailMessage.AlternateViews.Add(alternate);

            return mailMessage;
        }
    }
}