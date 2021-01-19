namespace FlexKids.Core.FlexKidsClient
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FlexKids.Core.Scheduler;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// FlexKids client using HttpClient.
    /// </summary>
    public class HttpFlexKidsClient : IFlexKidsClient
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly FlexKidsHttpClientConfig _config;
        private bool _isLoggedIn;

        public HttpFlexKidsClient(ILogger logger, HttpClient httpClient, FlexKidsHttpClientConfig config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<string> GetSchedulePage(int id)
        {
            if (!_isLoggedIn)
            {
                await Login();
            }

            var urlSchedule = string.Format(_config.HostUrl + "/personeel/rooster/week?week={0}", id);
            return await DownloadPageAsString(urlSchedule);
        }

        public async Task<string> GetAvailableSchedulesPage()
        {
            if (!_isLoggedIn)
            {
                await Login();
            }

            return await DownloadPageAsString(_config.HostUrl + "/personeel/rooster/index");
        }

        public void Dispose()
        {
            // do nothing
        }

        private async Task Login()
        {
            var requestParams = new NameValueCollection
                {
                    { "username", _config.Username },
                    { "password", _config.Password },
                    { "role", "4" },
                    { "login", "Log in" },
                };

            _ = await PostValues(_config.HostUrl + "/user/process", requestParams);

            _isLoggedIn = true;
        }

        private async Task<byte[]> PostValues(string address, NameValueCollection data)
        {
            var nameValueCollection = data.AllKeys.Select(key => new KeyValuePair<string, string>(key, data.Get(key))).ToList();
            HttpResponseMessage result = await _httpClient.PostAsync(address, new FormUrlEncodedContent(nameValueCollection));
            return await result.Content.ReadAsByteArrayAsync();
        }

        private async Task<string> DownloadPageAsString(string address)
        {
            HttpResponseMessage result = await _httpClient.GetAsync(address);
            return await result.Content.ReadAsStringAsync();
        }
    }
}
