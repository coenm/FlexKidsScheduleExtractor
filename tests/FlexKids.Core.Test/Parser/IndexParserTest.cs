namespace FlexKids.Core.Test.Parser
{
    using System.IO;
    using System.Threading.Tasks;
    using FlexKids.Core.Parser;
    using FlexKids.Core.Scheduler.Model;
    using Microsoft.Extensions.Logging.Abstractions;
    using VerifyXunit;
    using Xunit;

    [UsesVerify]
    public class IndexParserTest
    {
        private const string RESOURCE_DIRECTORY = "resources/2020";

        [Fact]
        public async Task IndexPageTest()
        {
            // arrange
            var htmlContent = GetFileContent("index.txt");
            var indexParser = new IndexParser(NullLogger.Instance);

            // act
            IndexContent indexContent = indexParser.Parse(htmlContent);

            // assert
            await Verifier.Verify(indexContent);
        }

        private static string GetFileContent(string filename)
        {
            var file = Path.Combine(RESOURCE_DIRECTORY, filename);
            Assert.True(File.Exists(file));
            return File.ReadAllText(file);
        }
    }
}