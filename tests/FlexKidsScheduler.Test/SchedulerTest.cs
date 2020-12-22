namespace FlexKidsScheduler.Test
{
    using System;
    using System.Collections.Generic;
    using FakeItEasy;
    using FlexKidsScheduler.Model;
    using Repository;
    using Repository.Model;
    using Xunit;

    public class SchedulerTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetChangesWithNoChangesReturnsEmptyListTest(bool isLoggedIn)
        {
            // arrange
            IFlexKidsConnection flexKidsConnection = A.Fake<IFlexKidsConnection>();
            IKseParser parser = A.Fake<IKseParser>();
            _ = A.CallTo(() => parser.GetIndexContent(A<string>._))
             .Returns(new IndexContent
             {
                 Email = "a@b.nl",
                 IsLoggedin = isLoggedIn,
                 Weeks = new Dictionary<int, WeekItem>(),
             });
            IScheduleRepository scheduleRepository = A.Dummy<IScheduleRepository>();
            IHash hash = A.Dummy<IHash>();
            var sut = new Scheduler(flexKidsConnection, parser, scheduleRepository, hash);

            // act
            IEnumerable<ScheduleDiff> result = sut.GetChanges();

            // assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _ = A.CallTo(() => flexKidsConnection.GetAvailableSchedulesPage()).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => parser.GetIndexContent(A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => flexKidsConnection.GetSchedulePage(A<int>._)).MustNotHaveHappened();
            A.CallTo(hash).MustNotHaveHappened();
            A.CallTo(scheduleRepository).MustNotHaveHappened();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetChangesWithOneScheduleWichAlreadyExistsAndDidntChangeReturnsEmptyListTest(bool isLoggedIn)
        {
            // arrange
            var weeks = new Dictionary<int, WeekItem>
                {
                    { 0, new WeekItem(6, 2015) },
                };

            IFlexKidsConnection flexKidsConnection = A.Fake<IFlexKidsConnection>();
            IKseParser parser = A.Fake<IKseParser>();
            IScheduleRepository scheduleRepository = A.Dummy<IScheduleRepository>();
            IHash hash = A.Dummy<IHash>();
            var sut = new Scheduler(flexKidsConnection, parser, scheduleRepository, hash);

            _ = A.CallTo(() => parser.GetIndexContent(A<string>._))
             .Returns(new IndexContent
             {
                 Email = "a@b.nl",
                 IsLoggedin = isLoggedIn,
                 Weeks = weeks,
             });
            _ = A.CallTo(() => flexKidsConnection.GetSchedulePage(A<int>.That.IsEqualTo(0))).Returns("GetSchedulePage0");
            _ = A.CallTo(() => hash.Hash(A<string>.That.IsEqualTo("GetSchedulePage0"))).Returns("hash0");
            _ = A.CallTo(() => scheduleRepository.GetWeek(A<int>.That.IsEqualTo(2015), A<int>.That.IsEqualTo(6))).Returns(new Week
            {
                Hash = "hash0",
            });

            // act
            IEnumerable<ScheduleDiff> result = sut.GetChanges();

            // assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _ = A.CallTo(() => flexKidsConnection.GetAvailableSchedulesPage()).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => parser.GetIndexContent(A<string>._)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => flexKidsConnection.GetSchedulePage(A<int>.That.IsEqualTo(0))).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => hash.Hash(A<string>.That.IsEqualTo("GetSchedulePage0"))).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => scheduleRepository.GetWeek(A<int>.That.IsEqualTo(2015), A<int>.That.IsEqualTo(6))).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetaChangesWithOneScheduleWichAlreadyExistsAndDidNotChangeReturnsEmptyListTest(bool isLoggedIn)
        {
            // arrange
            var weeks = new Dictionary<int, WeekItem>
                {
                    { 0, new WeekItem(6, 2015) },
                };
            var weekOld = new Week
                {
                    Hash = "hashOld",
                    Id = 1,
                    WeekNr = 6,
                    Year = 2015,
                };
            var weekNew = new Week
                {
                    Hash = "hashNew",
                    Id = 1,
                    WeekNr = 6,
                    Year = 2015,
                };
            weekNew.Schedules = new List<Schedule>
                {
                    new Schedule()
                        {
                            Id = 2,
                            Location = "LocA",
                            Week = weekNew,
                            WeekId = weekNew.Id,
                            StartDateTime = new DateTime(2015, 01, 02, 12, 13, 14),
                            EndDateTime = new DateTime(2015, 01, 02, 17, 13, 14),
                        },
                };

            IFlexKidsConnection flexKidsConnection = A.Fake<IFlexKidsConnection>();
            IKseParser parser = A.Fake<IKseParser>();
            IScheduleRepository scheduleRepository = A.Dummy<IScheduleRepository>();
            IHash hash = A.Dummy<IHash>();
            var sut = new Scheduler(flexKidsConnection, parser, scheduleRepository, hash);

            _ = A.CallTo(() => parser.GetIndexContent(A<string>._))
                 .Returns(new IndexContent
                     {
                         Email = "a@b.nl",
                         IsLoggedin = isLoggedIn,
                         Weeks = weeks,
                     });
            _ = A.CallTo(() => flexKidsConnection.GetSchedulePage(A<int>.That.IsEqualTo(0))).Returns("GetSchedulePage0");
            _ = A.CallTo(() => hash.Hash(A<string>.That.IsEqualTo("GetSchedulePage0"))).Returns(weekNew.Hash);
            _ = A.CallTo(() => scheduleRepository.GetWeek(A<int>.That.IsEqualTo(2015), A<int>.That.IsEqualTo(6))).Returns(weekOld);
            _ = A.CallTo(() => scheduleRepository.Update(A<Week>._, A<Week>._)).Returns(weekNew);

            // act
            IEnumerable<ScheduleDiff> result = sut.GetChanges();

            // assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _ = A.CallTo(() => flexKidsConnection.GetAvailableSchedulesPage()).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => parser.GetIndexContent(A<string>._)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => flexKidsConnection.GetSchedulePage(A<int>.That.IsEqualTo(0))).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => hash.Hash(A<string>.That.IsEqualTo("GetSchedulePage0"))).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => scheduleRepository.GetWeek(A<int>.That.IsEqualTo(2015), A<int>.That.IsEqualTo(6))).MustHaveHappenedOnceExactly();
        }
    }
}