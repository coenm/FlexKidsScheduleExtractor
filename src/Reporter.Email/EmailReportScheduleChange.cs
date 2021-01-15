namespace Reporter.Email
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;
    using System.Net.Mime;
    using System.Text;
    using FlexKidsScheduler;
    using FlexKidsScheduler.Model;
    using NLog;

    public class EmailReportScheduleChange : IReportScheduleChange
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IFlexKidsConfig _flexKidsConfig;
        private readonly IEmailService _emailService;

        public EmailReportScheduleChange(IFlexKidsConfig flexKidsConfig, IEmailService emailService)
        {
            _flexKidsConfig = flexKidsConfig ?? throw new ArgumentNullException(nameof(flexKidsConfig));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public bool HandleChange(IReadOnlyList<ScheduleDiff> schedule)
        {
            if (schedule == null || !schedule.Any())
            {
                return true;
            }

            try
            {
                var orderedSchedule = schedule.OrderBy(x => x.Start).ThenBy(x => x.Status).ToArray();
                var subject = "Werkrooster voor week " + orderedSchedule[0].Schedule.Week.WeekNr;
                var schedulePlain = EmailContentBuilder.ScheduleToPlainTextString(orderedSchedule);
                var scheduleHtml = EmailContentBuilder.ScheduleToHtmlString(orderedSchedule);

                var mm = CreateMailMessage(subject, schedulePlain, scheduleHtml);
                _emailService.Send(mm);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Something went wrong sending an email with the schedule.");
                return false;
            }

            return true;
        }

        private MailMessage CreateMailMessage(string subject, string schedulePlain, string scheduleHtml)
        {
            var from = new MailAddress(_flexKidsConfig.EmailFrom, "FlexKids rooster");
            var toEmail1 = new MailAddress(_flexKidsConfig.EmailTo1, _flexKidsConfig.EmailToName1);
            var toEmail2 = new MailAddress(_flexKidsConfig.EmailTo2, _flexKidsConfig.EmailToName2);
            var mm = new MailMessage(from, toEmail1)
                {
                    Subject = subject,
                    Body = schedulePlain,
                    BodyEncoding = Encoding.UTF8,
                    DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure,
                };
            mm.To.Add(toEmail2);

            var mimeType = new ContentType("text/html");
            var alternate = AlternateView.CreateAlternateViewFromString(scheduleHtml, mimeType);
            mm.AlternateViews.Add(alternate);

            return mm;
        }
    }
}