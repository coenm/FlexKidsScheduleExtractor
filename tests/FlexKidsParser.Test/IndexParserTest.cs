namespace FlexKidsParser.Test
{
    using System.Collections.Generic;
    using System.IO;
    using FlexKidsParser.Model;
    using Xunit;

    public class IndexParserTest
    {
        private const string RESOURCE_DIRECTORY = "resources/2020";

        [Fact]
        public void IndexPageTest()
        {
            // arrange
            const string EXPECTED_EMAIL = "fake.login@github.com";
            const bool EXPECTED_IS_LOGGEDIN = true;
            var expectedWeeks = new Dictionary<int, WeekItem>
                {
                    { 0, new WeekItem(50, 2020) },
                    { 1, new WeekItem(51, 2020) },
                    { 2, new WeekItem(52, 2020) },
                };

            // act
            var htmlContent = GetFileContent("index.html");
            var indexParser = new IndexParser(htmlContent);
            IndexContent indexContent = indexParser.Parse();

            // assert
            Assert.NotNull(indexContent);
            Assert.Equal(indexContent.Email, EXPECTED_EMAIL);
            Assert.Equal(indexContent.IsLoggedin, EXPECTED_IS_LOGGEDIN);
            Assert.Equal(indexContent.Weeks.Count, expectedWeeks.Count);

            foreach (KeyValuePair<int, WeekItem> item in indexContent.Weeks)
            {
                Assert.True(expectedWeeks.ContainsKey(item.Key));
                Assert.Equal(expectedWeeks[item.Key].Year, item.Value.Year);
                Assert.Equal(expectedWeeks[item.Key].WeekNr, item.Value.WeekNr);
            }
        }

        private static string GetFileContent(string filename)
        {
            var file = Path.Combine(RESOURCE_DIRECTORY, filename);
            Assert.True(File.Exists(file));
            return File.ReadAllText(file);
        }
    }
}