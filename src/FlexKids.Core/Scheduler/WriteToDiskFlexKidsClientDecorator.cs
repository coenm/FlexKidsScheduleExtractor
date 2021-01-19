namespace FlexKids.Core.Scheduler
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class WriteToDiskFlexKidsClientDecorator : IFlexKidsClient
    {
        private readonly IFlexKidsClient _flexKidsClientImplementation;
        private readonly WriteToDiskOptions _options;

        public WriteToDiskFlexKidsClientDecorator(IFlexKidsClient flexKidsClientImplementation, WriteToDiskOptions options)
        {
            _flexKidsClientImplementation = flexKidsClientImplementation ?? throw new ArgumentNullException(nameof(flexKidsClientImplementation));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void Dispose()
        {
            _flexKidsClientImplementation.Dispose();
        }

        public async Task<string> GetSchedulePage(int id)
        {
            var result = await _flexKidsClientImplementation.GetSchedulePage(id);

            try
            {
                _ = Directory.CreateDirectory(_options.Directory);
                File.WriteAllText(Path.Combine(_options.Directory, $"page_{id}.html"), result, Encoding.UTF8);
            }
            catch (Exception)
            {
                // swallow
            }

            return result;
        }

        public async Task<string> GetAvailableSchedulesPage()
        {
            var result = await _flexKidsClientImplementation.GetAvailableSchedulesPage();

            try
            {
                _ = Directory.CreateDirectory(_options.Directory);
                File.WriteAllText(Path.Combine(_options.Directory, "index.html"), result, Encoding.UTF8);
            }
            catch (Exception)
            {
                // swallow
            }

            return result;
        }
    }
}