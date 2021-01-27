namespace FlexKids.Core.FlexKidsClient
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using FlexKids.Core.Interfaces;

    /// <summary>
    /// FlexKids client using HttpClient.
    /// </summary>
    public class HttpFlexKidsClient : IFlexKidsClient
    {
        private readonly HttpClient _httpClient;
        private readonly FlexKidsHttpClientConfig _config;
        private bool _isLoggedIn;

        public HttpFlexKidsClient(HttpClient httpClient, FlexKidsHttpClientConfig config)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<string> GetSchedulePage(int id, CancellationToken cancellationToken)
        {
            if (!_isLoggedIn)
            {
                await Login(cancellationToken);
            }

            var urlSchedule = string.Format(_config.HostUrl + "/personeel/rooster/week?week={0}", id);
            return await DownloadPageAsString(urlSchedule, cancellationToken);
        }

        public async Task<string> GetAvailableSchedulesPage(CancellationToken cancellationToken)
        {
            if (!_isLoggedIn)
            {
                await Login(cancellationToken);
            }

            return await DownloadPageAsString(_config.HostUrl + "/personeel/rooster/index", cancellationToken);
        }

        public void Dispose()
        {
            // do nothing
        }

        private async Task Login(CancellationToken cancellationToken)
        {
            var requestParams = new NameValueCollection
                {
                    { "username", _config.Username },
                    { "password", _config.Password },
                    { "role", "4" },
                    { "login", "Log in" },
                };

            _ = await PostValues(_config.HostUrl + "/user/process", requestParams, cancellationToken);

            _isLoggedIn = true;
        }

        private async Task<byte[]> PostValues(string address, NameValueCollection data, CancellationToken cancellationToken)
        {
            var nameValueCollection = data.AllKeys.Select(key => new KeyValuePair<string, string>(key, data.Get(key))).ToList();
            HttpResponseMessage result = await _httpClient.PostAsync(address, new FormUrlEncodedContent(nameValueCollection), cancellationToken);
            return await result.Content.ReadAsByteArrayAsync();
        }

        private async Task<string> DownloadPageAsString(string address, CancellationToken cancellationToken)
        {
            HttpResponseMessage result = await _httpClient.GetAsync(address, cancellationToken);
            return await result.Content.ReadAsStringAsync();
        }
    }
}
