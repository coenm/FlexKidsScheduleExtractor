namespace Reporter.Email.Test
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using FakeItEasy;
    using FlexKids.Core.Interfaces;
    using FlexKids.Core.Repository.Model;
    using FlexKids.Core.Scheduler.Model;
    using Microsoft.Extensions.Logging.Abstractions;
    using Reporter.Email;
    using Xunit;

    public class EmailReportScheduleChangeTest
    {
        private readonly WeekSchedule _weekSchedule = new WeekSchedule()
            {
                Id = 2,
                Year = 2012,
                WeekNumber = 23,
            };

        private readonly SingleShift _singleShiftA = new SingleShift()
            {
                Id = 1,
                Location = "Jacob",
                StartDateTime = new DateTime(2012, 4, 8, 8, 5, 4),
                EndDateTime = new DateTime(2012, 4, 8, 17, 5, 4),
            };

        private readonly SingleShift _singleShiftB = new SingleShift()
            {
                Id = 3,
                Location = "New York",
                StartDateTime = new DateTime(2012, 1, 8, 10, 5, 4),
                EndDateTime = new DateTime(2012, 1, 8, 12, 5, 4),
            };

        private readonly SingleShift _singleShiftC = new SingleShift()
            {
                Id = 6,
                Location = "Madrid",
                StartDateTime = new DateTime(2012, 4, 8, 08, 30, 0),
                EndDateTime = new DateTime(2012, 4, 8, 22, 0, 0),
            };

        public EmailReportScheduleChangeTest()
        {
            _singleShiftA.WeekSchedule = _weekSchedule;
            _singleShiftA.WeekScheduleId = _weekSchedule.Id;

            _singleShiftB.WeekSchedule = _weekSchedule;
            _singleShiftB.WeekScheduleId = _weekSchedule.Id;

            _singleShiftC.WeekSchedule = _weekSchedule;
            _singleShiftC.WeekScheduleId = _weekSchedule.Id;
        }

        [Fact]
        public async Task HandleChangeWithEmptyListTest()
        {
            // arrange
            IEmailService emailService = A.Fake<IEmailService>();
            var flexKidsConfig = new EmailConfig(new MailAddress("a@b.c", "x"), new MailAddress("a@b.c", "x"));
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
                    SingleShift = _singleShiftA,
                    Status = ScheduleStatus.Added,
                },
                new ScheduleDiff
                {
                    SingleShift = _singleShiftB,
                    Status = ScheduleStatus.Removed,
                },
                new ScheduleDiff
                {
                    SingleShift = _singleShiftC,
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