namespace FlexKidsConnection.Test
{
    using System.Collections.Specialized;
    using FakeItEasy;
    using Xunit;

    public class FlexKidsCookieWebClientTest
    {
        private readonly FlexKidsCookieConfig _config = new FlexKidsCookieConfig("https://abc.local/", "user", "pass");

        [Fact]
        public void GetSchedulePageTest()
        {
            // arrange
            var web = A.Fake<IWeb>();
            var sut = new FlexKidsCookieWebClient(web, _config);

            // act
            _ = sut.GetSchedulePage(1);

            // assert
            _ = A.CallTo(() => web.PostValues(A<string>._, A<NameValueCollection>._)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => web.DownloadPageAsString(A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void GetSchedulePageTwiceTest()
        {
            // arrange
            var web = A.Fake<IWeb>();
            var sut = new FlexKidsCookieWebClient(web, _config);

            // act
            _ = sut.GetSchedulePage(1);
            _ = sut.GetSchedulePage(2);

            // assert
            _ = A.CallTo(() => web.PostValues(A<string>._, A<NameValueCollection>._)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => web.DownloadPageAsString(A<string>._)).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public void GetAvailableSchedulesPageTest()
        {
            // arrange
            var response = "sdfdsf34IUHDSf834";
            var web = A.Fake<IWeb>();
            _ = A.CallTo(() => web.DownloadPageAsString(A<string>._)).Returns(response);
            var sut = new FlexKidsCookieWebClient(web, _config);

            // act
            var result = sut.GetAvailableSchedulesPage();

            // assert
            _ = A.CallTo(() => web.PostValues(A<string>._, A<NameValueCollection>._)).MustHaveHappenedOnceExactly();
            _ = A.CallTo(() => web.DownloadPageAsString(A<string>._)).MustHaveHappenedOnceExactly();
            Assert.Equal(result, response);
        }

        [Fact]
        public void DisposeTest()
        {
            // arrange
            var web = A.Fake<IWeb>();
            var sut = new FlexKidsCookieWebClient(web, _config);

            // act
            sut.Dispose();

            // assert
            _ = A.CallTo(() => web.Dispose()).MustHaveHappenedOnceExactly();
        }
    }
}
