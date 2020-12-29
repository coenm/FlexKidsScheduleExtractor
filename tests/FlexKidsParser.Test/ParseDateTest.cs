namespace FlexKidsParser.Test
{
    using System;
    using Xunit;
    using Sut = FlexKidsParser.Helper.ParseDate;

    public class ParseDateTest
    {
        [Theory]
        [InlineData("09:00", 9, 0)]
        [InlineData("23:11", 23, 11)]
        [InlineData("00:59", 0, 59)]
        public void AddStringTimeToDateTest(string input, int hour, int min)
        {
            // arrange
            var d = new DateTime(214, 1, 2, 0, 0, 0);

            // act
            DateTime result = Sut.AddStringTimeToDate(d, input);

            // assert
            Assert.Equal(result.Year, d.Year);
            Assert.Equal(result.Month, d.Month);
            Assert.Equal(result.Day, d.Day);
            Assert.Equal(result.Hour, hour);
            Assert.Equal(result.Minute, min);
        }

        [Theory]
        [InlineData("1100")]
        public void AddStringTimeToDateFail1Test(string input)
        {
            // arrange
            var d = new DateTime(214, 1, 2, 0, 0, 0);

            // act
            // assert
            _ = Assert.Throws<FormatException>(() => Sut.AddStringTimeToDate(d, input));
        }

        [Theory]
        [InlineData("-1:00", "Hours not in range")]
        [InlineData("24:00", "Hours not in range")]
        [InlineData("ab:00", "No hours found")]
        [InlineData("a1:00", "No hours found")]
        [InlineData("1a:00", "No hours found")]
        [InlineData(" :00", "No hours found")]
        [InlineData("1:-1", "Minutes not in range")]
        [InlineData("1:60", "Minutes not in range")]
        [InlineData("1:160", "Minutes not in range")]
        [InlineData("1:aa", "No minutes found")]
        [InlineData("1:a1", "No minutes found")]
        [InlineData("1:", "No minutes found")]
        public void AddStringTimeToDateFail2Test(string input, string expectedMsg)
        {
            // arrange
            var d = new DateTime(214, 1, 2, 0, 0, 0);

            // act
            // assert
            Exception ex = Assert.Throws<Exception>(() => Sut.AddStringTimeToDate(d, input));
            Assert.Equal(ex.Message, expectedMsg);
        }

        [Theory]
        [InlineData("09:00-13:30", 9, 0, 13, 30)]
        [InlineData("14:00-17:30", 14, 0, 17, 30)]
        public void CreateStartEndDateTimeTupleTest(string input, int startHour, int startMin, int endHour, int endMin)
        {
            // arrange
            var d = new DateTime(2014, 1, 2, 16, 17, 18);

            // act
            Tuple<DateTime, DateTime> result = Sut.CreateStartEndDateTimeTuple(d, input);

            // assert
            Assert.Equal(result.Item1, new DateTime(2014, 1, 2, startHour, startMin, 0));
            Assert.Equal(result.Item2, new DateTime(2014, 1, 2, endHour, endMin, 0));
        }

        [Theory]
        [InlineData("din 3-feb.", 2015, 2, 3)]
        [InlineData("maa 8-dec.", 2014, 12, 8)]
        [InlineData("din 9-dec.", 2014, 12, 9)]
        [InlineData("woe 10-dec.", 2014, 12, 10)]
        [InlineData("don 11-dec.", 2014, 12, 11)]
        [InlineData("vri 12-dec.", 2014, 12, 12)]
        [InlineData("vri 6-mrt.", 2015, 3, 06)]
        [InlineData("woe 29-apr.", 2015, 4, 29)]
        [InlineData("vri 12-mei.", 2015, 5, 12)]
        public void StringToDateTimeTest(string input, int year, int month, int day)
        {
            // arrange
            // act
            DateTime result = Sut.StringToDateTime(input, year);

            // assert
            Assert.Equal(result.Year, year);
            Assert.Equal(result.Month, month);
            Assert.Equal(result.Day, day);
        }

        [Theory]
        [InlineData("din 3 feb.")]
        [InlineData("din3feb.")]
        [InlineData("din-3-feb.")]
        [InlineData(" din 3-feb. ")]
        [InlineData("")]
        public void ParseDateFailure1Test(string input)
        {
            // arrange
            const int YEAR = 2015;

            // act
            // assert
            _ = Assert.Throws<FormatException>(() => Sut.StringToDateTime(input, YEAR));
        }

        [Theory]
        [InlineData("alpha.", "alpha")]
        [InlineData("alpha", "alpha")]
        [InlineData("alpha...", "alpha..")]
        [InlineData("alpha. ", "alpha. ")]
        [InlineData("", "")]
        public void TestRemoveLastCharIfDot(string test, string expectedResult)
        {
            // arrange

            // act
            var result = Sut.RemoveLastCharIfDot(test);

            // assert
            Assert.Equal(result, expectedResult);
        }
    }
}