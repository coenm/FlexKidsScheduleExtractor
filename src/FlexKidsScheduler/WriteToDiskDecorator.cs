namespace FlexKidsScheduler
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class WriteToDiskDecorator : IFlexKidsConnection
    {
        private readonly IFlexKidsConnection _flexKidsConnectionImplementation;
        private readonly WriteToDiskOptions _options;

        public WriteToDiskDecorator(IFlexKidsConnection flexKidsConnectionImplementation, WriteToDiskOptions options)
        {
            _flexKidsConnectionImplementation = flexKidsConnectionImplementation;
            _options = options;
        }

        public void Dispose()
        {
            _flexKidsConnectionImplementation.Dispose();
        }

        public async Task<string> GetSchedulePage(int id)
        {
            _ = Directory.CreateDirectory(_options.Directory);

            var result = await _flexKidsConnectionImplementation.GetSchedulePage(id);

            File.WriteAllText(Path.Combine(_options.Directory, $"page_{id}.html"), result, Encoding.UTF8);

            return result;
        }

        public async Task<string> GetAvailableSchedulesPage()
        {
            _ = Directory.CreateDirectory(_options.Directory);

            var result = await _flexKidsConnectionImplementation.GetAvailableSchedulesPage();
            File.WriteAllText(Path.Combine(_options.Directory, "index.html"), result, Encoding.UTF8);
            return result;
        }
    }
}