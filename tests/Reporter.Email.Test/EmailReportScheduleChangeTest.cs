namespace Reporter.Email.Test
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using FakeItEasy;
    using FlexKidsScheduler;
    using FlexKidsScheduler.Model;
    using Microsoft.Extensions.Logging.Abstractions;
    using Reporter.Email;
    using Repository.Model;
    using Xunit;

    public class EmailReportScheduleChangeTest
    {
        private readonly Week _week = new Week()
            {
                Id = 2,
                Hash = "abc123",
                Year = 2012,
                WeekNr = 23,
            };

        private readonly Schedule _scheduleA = new Schedule()
            {
                Id = 1,
                Location = "Jacob",
                StartDateTime = new DateTime(2012, 4, 8, 8, 5, 4),
                EndDateTime = new DateTime(2012, 4, 8, 17, 5, 4),
            };

        private readonly Schedule _scheduleB = new Schedule()
            {
                Id = 3,
                Location = "New York",
                StartDateTime = new DateTime(2012, 1, 8, 10, 5, 4),
                EndDateTime = new DateTime(2012, 1, 8, 12, 5, 4),
            };

        private readonly Schedule _scheduleC = new Schedule()
            {
                Id = 6,
                Location = "Madrid",
                StartDateTime = new DateTime(2012, 4, 8, 08, 30, 0),
                EndDateTime = new DateTime(2012, 4, 8, 22, 0, 0),
            };

        public EmailReportScheduleChangeTest()
        {
            _scheduleA.Week = _week;
            _scheduleA.WeekId = _week.Id;

            _scheduleB.Week = _week;
            _scheduleB.WeekId = _week.Id;

            _scheduleC.Week = _week;
            _scheduleC.WeekId = _week.Id;
        }

        [Fact]
        public async Task HandleChangeWithEmptyListTest()
        {
            // arrange
            IEmailService emailService = A.Fake<IEmailService>();
            EmailConfig flexKidsConfig = A.Fake<EmailConfig>();
            var sut = new EmailReportScheduleChange(flexKidsConfig, emailService, NullLogger.Instance);

            // act
            var result = await sut.HandleChange(null);

            // assert
            Assert.True(result);
        }

        [Fact]
        public async Task HandleChangeWithThreeItemsInListTest()
        {
            // arrange
            IEmailService emailService = A.Fake<IEmailService>();
            var flexKidsConfig = new EmailConfig(
                new MailAddress("from@me.com", "FlexKidsService"),
                new MailAddress("you@you.com", "you"),
                new MailAddress("and.you@you.com", "and you"));
            var sut = new EmailReportScheduleChange(flexKidsConfig, emailService, NullLogger.Instance);

            var scheduleDiff = new List<ScheduleDiff>()
            {
                new ScheduleDiff
                {
                    Schedule = _scheduleA,
                    Status = ScheduleStatus.Added,
                },
                new ScheduleDiff
                {
                    Schedule = _scheduleB,
                    Status = ScheduleStatus.Removed,
                },
                new ScheduleDiff
                {
                    Schedule = _scheduleC,
                    Status = ScheduleStatus.Unchanged,
                },
            };

            // act
            var result = await sut.HandleChange(scheduleDiff);

            // assert
            Assert.True(result);
            _ = A.CallTo(() => emailService.Send(A<System.Net.Mail.MailMessage>._)).MustHaveHappenedOnceExactly();
        }
    }
}