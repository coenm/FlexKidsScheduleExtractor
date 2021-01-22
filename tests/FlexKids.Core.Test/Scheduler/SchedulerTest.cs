namespace FlexKids.Core.Test.Scheduler
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FakeItEasy;
    using FlexKids.Core.Interfaces;
    using FlexKids.Core.Repository;
    using FlexKids.Core.Repository.Model;
    using FlexKids.Core.Scheduler;
    using FlexKids.Core.Scheduler.Model;
    using Xunit;

    public class SchedulerTest
    {
        [Theory]
        [InlineData("a@b.nl")]
        [InlineData("")]
        public async Task GetChangesWithNoChangesReturnsEmptyListTest(string email)
        {
            // arrange
            IFlexKidsClient flexKidsClient = A.Fake<IFlexKidsClient>();
            IKseParser parser = A.Fake<IKseParser>();
            _ = A.CallTo(() => parser.GetIndexContent(A<string>._))
                 .Returns(new IndexContent
                     {
                         Email = email,
                         Weeks = new Dictionary<int, WeekItem>(),
                     });

            IScheduleRepository scheduleRepository = A.Dummy<IScheduleRepository>();
            var sut = new Scheduler(flexKidsClient, parser, scheduleRepository);

            // act
            IEnumerable<ScheduleDiff> result = await sut.ProcessAsync();

            // assert
            Assert.Empty(result);
            _ = A.CallTo(() => flexKidsClient.GetAvailableSchedulesPage()).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => parser.GetIndexContent(A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => flexKidsClient.GetSchedulePage(A<int>._)).MustNotHaveHappened();
            A.CallTo(scheduleRepository).MustNotHaveHappened();
        }

        [Theory]
        [InlineData("a@b.nl")]
        [InlineData("")]
        public async Task GetChangesWithOneScheduleWhichAlreadyExistsAndDidNotChangeReturnsEmptyListTest(string email)
        {
            // arrange
            var weeks = new Dictionary<int, WeekItem>
                {
                    { 0, new WeekItem(6, 2015) },
                };

            IFlexKidsClient flexKidsClient = A.Fake<IFlexKidsClient>();
            IKseParser parser = A.Fake<IKseParser>();
            IScheduleRepository scheduleRepository = A.Dummy<IScheduleRepository>();
            var sut = new Scheduler(flexKidsClient, parser, scheduleRepository);

            _ = A.CallTo(() => parser.GetIndexContent(A<string>._))
                 .Returns(new IndexContent
                     {
                         Email = email,
                         Weeks = weeks,
                     });
            _ = A.CallTo(() => flexKidsClient.GetSchedulePage(0)).Returns("GetSchedulePage0");
            _ = A.CallTo(() => scheduleRepository.Get(2015, 6))
                 .Returns(new WeekSchedule { });

            // act
            IEnumerable<ScheduleDiff> result = await sut.ProcessAsync();

            // assert
            Assert.Empty(result);
            _ = A.CallTo(() => flexKidsClient.GetAvailableSchedulesPage()).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => parser.GetIndexContent(A<string>._)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => flexKidsClient.GetSchedulePage(0)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => scheduleRepository.Get(2015, 6)).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData("a@b.nl")]
        [InlineData("")]
        public async Task GetaChangesWithOneScheduleWhichAlreadyExistsAndDidNotChangeReturnsEmptyListTest(string email)
        {
            // arrange
            var weeks = new Dictionary<int, WeekItem>
                {
                    { 0, new WeekItem(6, 2015) },
                };
            var weekOld = new WeekSchedule
                {
                    Id = 1,
                    WeekNumber = 6,
                    Year = 2015,
                };
            var weekNew = new WeekSchedule
                {
                    Id = 1,
                    WeekNumber = 6,
                    Year = 2015,
                };

            IFlexKidsClient flexKidsClient = A.Fake<IFlexKidsClient>();
            IKseParser parser = A.Fake<IKseParser>();
            IScheduleRepository scheduleRepository = A.Dummy<IScheduleRepository>();
            var sut = new Scheduler(flexKidsClient, parser, scheduleRepository);

            _ = A.CallTo(() => parser.GetIndexContent(A<string>._))
                 .Returns(new IndexContent
                     {
                         Email = email,
                         Weeks = weeks,
                     });
            _ = A.CallTo(() => flexKidsClient.GetSchedulePage(0)).Returns("GetSchedulePage0");
            _ = A.CallTo(() => scheduleRepository.Get(2015, 6)).Returns(weekOld);
            _ = A.CallTo(() => scheduleRepository.Save(A<WeekSchedule>._)).Returns(weekNew);

            // act
            IEnumerable<ScheduleDiff> result = await sut.ProcessAsync();

            // assert
            Assert.Empty(result);
            _ = A.CallTo(() => flexKidsClient.GetAvailableSchedulesPage()).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => parser.GetIndexContent(A<string>._)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => flexKidsClient.GetSchedulePage(0)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => scheduleRepository.Get(2015, 6)).MustHaveHappenedOnceExactly();
        }
    }
}