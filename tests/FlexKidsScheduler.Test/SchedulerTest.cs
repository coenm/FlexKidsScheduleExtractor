namespace FlexKidsScheduler.Test
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
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
        public async Task GetChangesWithNoChangesReturnsEmptyListTest(bool isLoggedIn)
        {
            // arrange
            IFlexKidsClient flexKidsClient = A.Fake<IFlexKidsClient>();
            IKseParser parser = A.Fake<IKseParser>();
            _ = A.CallTo(() => parser.GetIndexContent(A<string>._))
             .Returns(new IndexContent
             {
                 Email = "a@b.nl",
                 IsLoggedIn = isLoggedIn,
                 Weeks = new Dictionary<int, WeekItem>(),
             });
            IScheduleRepository scheduleRepository = A.Dummy<IScheduleRepository>();
            IHash hash = A.Dummy<IHash>();
            var sut = new Scheduler(flexKidsClient, parser, scheduleRepository, hash);

            // act
            IEnumerable<ScheduleDiff> result = await sut.GetChanges();

            // assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _ = A.CallTo(() => flexKidsClient.GetAvailableSchedulesPage()).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => parser.GetIndexContent(A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => flexKidsClient.GetSchedulePage(A<int>._)).MustNotHaveHappened();
            A.CallTo(hash).MustNotHaveHappened();
            A.CallTo(scheduleRepository).MustNotHaveHappened();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetChangesWithOneScheduleWhichAlreadyExistsAndDidNotChangeReturnsEmptyListTest(bool isLoggedIn)
        {
            // arrange
            var weeks = new Dictionary<int, WeekItem>
                {
                    { 0, new WeekItem(6, 2015) },
                };

            IFlexKidsClient flexKidsClient = A.Fake<IFlexKidsClient>();
            IKseParser parser = A.Fake<IKseParser>();
            IScheduleRepository scheduleRepository = A.Dummy<IScheduleRepository>();
            IHash hash = A.Dummy<IHash>();
            var sut = new Scheduler(flexKidsClient, parser, scheduleRepository, hash);

            _ = A.CallTo(() => parser.GetIndexContent(A<string>._))
             .Returns(new IndexContent
             {
                 Email = "a@b.nl",
                 IsLoggedIn = isLoggedIn,
                 Weeks = weeks,
             });
            _ = A.CallTo(() => flexKidsClient.GetSchedulePage(0)).Returns("GetSchedulePage0");
            _ = A.CallTo(() => hash.Hash("GetSchedulePage0")).Returns("hash0");
            _ = A.CallTo(() => scheduleRepository.GetWeek(2015, 6)).Returns(new Week
            {
                Hash = "hash0",
            });

            // act
            IEnumerable<ScheduleDiff> result = await sut.GetChanges();

            // assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _ = A.CallTo(() => flexKidsClient.GetAvailableSchedulesPage()).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => parser.GetIndexContent(A<string>._)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => flexKidsClient.GetSchedulePage(0)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => hash.Hash("GetSchedulePage0")).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => scheduleRepository.GetWeek(2015, 6)).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetaChangesWithOneScheduleWhichAlreadyExistsAndDidNotChangeReturnsEmptyListTest(bool isLoggedIn)
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

            IFlexKidsClient flexKidsClient = A.Fake<IFlexKidsClient>();
            IKseParser parser = A.Fake<IKseParser>();
            IScheduleRepository scheduleRepository = A.Dummy<IScheduleRepository>();
            IHash hash = A.Dummy<IHash>();
            var sut = new Scheduler(flexKidsClient, parser, scheduleRepository, hash);

            _ = A.CallTo(() => parser.GetIndexContent(A<string>._))
                 .Returns(new IndexContent
                     {
                         Email = "a@b.nl",
                         IsLoggedIn = isLoggedIn,
                         Weeks = weeks,
                     });
            _ = A.CallTo(() => flexKidsClient.GetSchedulePage(0)).Returns("GetSchedulePage0");
            _ = A.CallTo(() => hash.Hash("GetSchedulePage0")).Returns(weekNew.Hash);
            _ = A.CallTo(() => scheduleRepository.GetWeek(2015, 6)).Returns(weekOld);
            _ = A.CallTo(() => scheduleRepository.Update(A<Week>._)).Returns(weekNew);

            // act
            IEnumerable<ScheduleDiff> result = await sut.GetChanges();

            // assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _ = A.CallTo(() => flexKidsClient.GetAvailableSchedulesPage()).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => parser.GetIndexContent(A<string>._)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => flexKidsClient.GetSchedulePage(0)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => hash.Hash("GetSchedulePage0")).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => scheduleRepository.GetWeek(2015, 6)).MustHaveHappenedOnceExactly();
        }
    }
}