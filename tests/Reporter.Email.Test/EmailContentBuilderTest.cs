namespace Reporter.Email.Test
{
    using System;
    using FlexKids.Core.Repository.Model;
    using FlexKids.Core.Scheduler.Model;
    using FluentAssertions;
    using Xunit;
    using Sut = Reporter.Email.EmailContentBuilder;

    public class EmailContentBuilderTest
    {
        private readonly WeekSchedule _weekSchedule = new WeekSchedule()
            {
                Id = 2,
                Hash = "sdfskdf83",
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
            _singleShiftA.WeekId = _weekSchedule.Id;

            _singleShiftB.WeekSchedule = _weekSchedule;
            _singleShiftB.WeekId = _weekSchedule.Id;

            _singleShiftC.WeekSchedule = _weekSchedule;
            _singleShiftC.WeekId = _weekSchedule.Id;
        }

        [Fact]
        public void ScheduleToPlainTextStringWithEmptyListReturnsEmptyStringTest()
        {
            // arrange
            var scheduleDiff = new ScheduleDiff[0];

            // act
            var result = Sut.ScheduleToPlainTextString(scheduleDiff);

            // assert
            Assert.Empty(result);
        }

        [Fact]
        public void ScheduleToPlainTextStringWithThreeItemsInListReturnsFormattedStringTest()
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
            var expected = string.Empty;
            expected += "+ 08-04 08:05-17:05 Jacob" + Environment.NewLine;
            expected += "- 08-01 10:05-12:05 New York" + Environment.NewLine;
            expected += "= 08-04 08:30-22:00 Madrid" + Environment.NewLine;
            _ = result.Should().Be(expected);
        }

        [Fact]
        public void ScheduleToHtmlStringWithEmptyListReturnsEmptyStringTest()
        {
            // arrange
            var scheduleDiff = new ScheduleDiff[] { };

            // act
            var result = Sut.ScheduleToHtmlString(scheduleDiff);

            // assert
            _ = result.Should().BeEmpty();
        }

        [Fact]
        public void ScheduleToHtmlStringWithThreeItemsInListReturnsFormattedStringTest()
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
            var expected = @"
<p>Hier is het rooster voor week 23:</p>
<table style='border: 1px solid black; border-collapse:collapse;'>
<tr style='text-align:left; padding:0px 5px; border: 1px solid black;'>
<td style='text-align:center; padding:0px 5px; border: 1px solid black;'></td>
<td colspan=2 style='text-align:left; padding:0px 5px; border: 1px solid black;'><b>Dag</b></td>
<td colspan=3 style='text-align:left; padding:0px 5px; border: 1px solid black;'><b>Tijd</b></td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black;'><b>Locatie</b></td>
</tr>
<tr style='text-align:left; padding:0px 5px; border: 1px solid black;'>
<td style='text-align:center; padding:0px 5px; border: 1px solid black;'>+</td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black; border-right:hidden;'>zo</td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black;'>08-04</td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black; text-align: right; padding-right:0px;'>08:05</td>
<td style='text-align:center; padding:0px 5px; border: 1px solid black; border-left: hidden; border-right: hidden;'>-</td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black; padding-left:0px;'>17:05</td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black;'>Jacob</td>
</tr>
<tr style='text-align:left; padding:0px 5px; border: 1px solid black;'>
<td style='text-align:center; padding:0px 5px; border: 1px solid black;'>-</td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black;text-decoration: line-through; border-right:hidden;'>zo</td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black;text-decoration: line-through;'>08-01</td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black;text-decoration: line-through; text-align: right; padding-right:0px;'>10:05</td>
<td style='text-align:center; padding:0px 5px; border: 1px solid black; border-left: hidden; border-right: hidden;'>-</td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black;text-decoration: line-through; padding-left:0px;'>12:05</td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black;text-decoration: line-through;'>New York</td>
</tr>
<tr style='text-align:left; padding:0px 5px; border: 1px solid black;'>
<td style='text-align:center; padding:0px 5px; border: 1px solid black;'>=</td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black; border-right:hidden;'>zo</td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black;'>08-04</td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black; text-align: right; padding-right:0px;'>08:30</td>
<td style='text-align:center; padding:0px 5px; border: 1px solid black; border-left: hidden; border-right: hidden;'>-</td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black; padding-left:0px;'>22:00</td>
<td style='text-align:left; padding:0px 5px; border: 1px solid black;'>Madrid</td>
</tr>
</table>
</p>";
            _ = result.Trim().Should().Be(expected.Trim());
        }
    }
}
