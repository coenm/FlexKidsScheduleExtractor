namespace FlexKidsParser.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using FlexKidsParser.Model;
    using Xunit;

    public class ScheduleParserTest
    {
        private const string RESOURCE_DIRECTORY = "resources";

        [Fact]
        public void Schedule2020Page0Test()
        {
            // arrange
            const int YEAR = 2020;

            var expectedItem0 = new ScheduleItem
                {
                    Start = new DateTime(YEAR, 12, 07, 8, 30, 0),
                    End = new DateTime(YEAR, 12, 07, 18, 0, 0),
                    Location = "Aapjes",
                };

            var expectedItem1 = new ScheduleItem
                {
                    Start = new DateTime(YEAR, 12, 08, 7, 45, 0),
                    End = new DateTime(YEAR, 12, 08, 14, 30, 0),
                    Location = "Aapjes",
                };

            var expectedItem2 = new ScheduleItem
                {
                    Start = new DateTime(YEAR, 12, 08, 14, 30, 0),
                    End = new DateTime(YEAR, 12, 08, 17, 30, 0),
                    Location = "Verhuizing groepen",
                };

            var expectedItem3 = new ScheduleItem
                {
                    Start = new DateTime(YEAR, 12, 09, 16, 30, 0),
                    End = new DateTime(YEAR, 12, 09, 18, 30, 0),
                    Location = "Verhuizing groepen",
                };

            // act
            var htmlContent = GetFileContent("2020/page_0.html");
            var contentParser = new ScheduleParser(htmlContent, YEAR);
            List<ScheduleItem> schedule = contentParser.GetScheduleFromContent();

            // assert
            Assert.NotNull(schedule);
            Assert.Equal(4, schedule.Count);
            AssertSchedule(schedule[0], expectedItem0);
            AssertSchedule(schedule[1], expectedItem1);
            AssertSchedule(schedule[2], expectedItem2);
            AssertSchedule(schedule[3], expectedItem3);
        }

        private static void AssertSchedule(ScheduleItem itemA, ScheduleItem itemB)
        {
            Assert.Equal(itemA.End, itemB.End);
            Assert.Equal(itemA.Start, itemB.Start);
            Assert.Equal(itemA.Location, itemB.Location);
        }

        private static string GetFileContent(string filename)
        {
            var file = Path.Combine(RESOURCE_DIRECTORY, filename);
            Assert.True(File.Exists(file));
            return File.ReadAllText(file);
        }
    }
}