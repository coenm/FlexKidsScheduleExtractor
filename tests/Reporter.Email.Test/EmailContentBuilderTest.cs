namespace Reporter.Email.Test
{
    using System;
    using System.Threading.Tasks;
    using FlexKids.Core.Repository.Model;
    using FlexKids.Core.Scheduler.Model;
    using FluentAssertions;
    using VerifyXunit;
    using Xunit;
    using Sut = Reporter.Email.EmailContentBuilder;

    [UsesVerify]
    public class EmailContentBuilderTest
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

        public EmailContentBuilderTest()
        {
            _singleShiftA.WeekSchedule = _weekSchedule;
            _singleShiftA.WeekScheduleId = _weekSchedule.Id;

            _singleShiftB.WeekSchedule = _weekSchedule;
            _singleShiftB.WeekScheduleId = _weekSchedule.Id;

            _singleShiftC.WeekSchedule = _weekSchedule;
            _singleShiftC.WeekScheduleId = _weekSchedule.Id;
        }

        [Fact]
        public void ScheduleToPlainTextStringWithEmptyListReturnsEmptyStringTest()
        {
            // arrange
            ScheduleDiff[] scheduleDiff = Array.Empty<ScheduleDiff>();

            // act
            var result = Sut.ScheduleToPlainTextString(scheduleDiff);

            // assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task ScheduleToPlainTextStringWithThreeItemsInListReturnsFormattedStringTest()
        {
            // arrange
            var scheduleDiff = new ScheduleDiff[]
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
            var result = Sut.ScheduleToPlainTextString(scheduleDiff);

            // assert
            await Verifier.Verify(result);
        }

        [Fact]
        public void ScheduleToHtmlStringWithEmptyListReturnsEmptyStringTest()
        {
            // arrange
            ScheduleDiff[] scheduleDiff = Array.Empty<ScheduleDiff>();

            // act
            var result = Sut.ScheduleToHtmlString(scheduleDiff);

            // assert
            _ = result.Should().BeEmpty();
        }

        [Fact]
        public async Task ScheduleToHtmlStringWithThreeItemsInListReturnsFormattedStringTest()
        {
            // arrange
            var scheduleDiff = new ScheduleDiff[]
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
            var result = Sut.ScheduleToHtmlString(scheduleDiff);

            // assert
            await Verifier.Verify(result);
        }
    }
}
