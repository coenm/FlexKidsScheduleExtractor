namespace FlexKidsScheduler
{
    using System;
    using System.IO;
    using System.Text;

    public interface IFlexKidsConnection : IDisposable
    {
        string GetSchedulePage(int id);

        string GetAvailableSchedulesPage();
    }

    public class WriteToDiskOptions
    {
        public string Directory { get; set; }
    }

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

        public string GetSchedulePage(int id)
        {
            var result = _flexKidsConnectionImplementation.GetSchedulePage(id);

            File.WriteAllText(Path.Combine(_options.Directory, $"page_{id}.html"), result, Encoding.UTF8);

            return result;
        }

        public string GetAvailableSchedulesPage()
        {
            var result =  _flexKidsConnectionImplementation.GetAvailableSchedulesPage();
            File.WriteAllText(Path.Combine(_options.Directory, "index.html"), result, Encoding.UTF8);
            return result;
        }
    }
}