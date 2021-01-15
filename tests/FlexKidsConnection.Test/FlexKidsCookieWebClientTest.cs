namespace FlexKidsConnection.Test
{
    using System.Collections.Specialized;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    public class FlexKidsCookieWebClientTest
    {
        private readonly FlexKidsCookieConfig _config = new FlexKidsCookieConfig("https://abc.local/", "user", "pass");

        [Fact]
        public async Task GetSchedulePageTest()
        {
            // arrange
            var web = A.Fake<IWeb>();
            var sut = new FlexKidsCookieWebClient(web, _config);

            // act
            _ = await sut.GetSchedulePage(1);

            // assert
            _ = A.CallTo(() => web.PostValues(A<string>._, A<NameValueCollection>._)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => web.DownloadPageAsString(A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetSchedulePageTwiceTest()
        {
            // arrange
            var web = A.Fake<IWeb>();
            var sut = new FlexKidsCookieWebClient(web, _config);

            // act
            _ = await sut.GetSchedulePage(1);
            _ = await sut.GetSchedulePage(2);

            // assert
            _ = A.CallTo(() => web.PostValues(A<string>._, A<NameValueCollection>._)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => web.DownloadPageAsString(A<string>._)).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task GetAvailableSchedulesPageTest()
        {
            // arrange
            var response = "sdfdsf34IUHDSf834";
            var web = A.Fake<IWeb>();
            _ = A.CallTo(() => web.DownloadPageAsString(A<string>._)).Returns(Task.FromResult(response));
            var sut = new FlexKidsCookieWebClient(web, _config);

            // act
            var result = await sut.GetAvailableSchedulesPage();

            // assert
            _ = A.CallTo(() => web.PostValues(A<string>._, A<NameValueCollection>._)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => web.DownloadPageAsString(A<string>._)).MustHaveHappenedOnceExactly();
            Assert.Equal(result, response);
        }
    }
}
