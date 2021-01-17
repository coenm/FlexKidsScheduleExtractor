namespace Reporter.Email.Test
{
    using System;
    using System.Collections.Generic;
    using FakeItEasy;
    using FlexKidsScheduler;
    using FlexKidsScheduler.Model;
    using Reporter.Email;
    using Repository.Model;
    using Xunit;

    public class EmailReportScheduleChangeTest
    {
        private readonly Week _week = new Week()
            {
                Id = 2,
                Hash = "sdfskdf83",
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
        public void HandleChangeWithEmptyListTest()
        {
            // arrange
            var emailService = A.Fake<IEmailService>();
            var flexKidsConfig = A.Fake<EmailConfig>();
            var sut = new EmailReportScheduleChange(flexKidsConfig, emailService);

            // act
            var result = sut.HandleChange(null);

            // assert
            Assert.True(result);
        }

        [Fact]
        public void HandleChangeWithThreeItemsInListTest()
        {
            // arrange
            IEmailService emailService = A.Fake<IEmailService>();
            var flexKidsConfig = new EmailConfig("a@b.com", "a@b.com", string.Empty, "a@b.com", string.Empty);
            var sut = new EmailReportScheduleChange(flexKidsConfig, emailService);

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
            var result = sut.HandleChange(scheduleDiff);

            // assert
            Assert.True(result);
            _ = A.CallTo(() => emailService.Send(A<System.Net.Mail.MailMessage>._)).MustHaveHappenedOnceExactly();
        }
    }
}