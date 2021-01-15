namespace FlexKids.Console
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FlexKidsConnection;

    public class WebClientAdapter : IWeb, IDisposable
    {
        private HttpClient _webclient;

        public WebClientAdapter()
        {
            _webclient = new HttpClient();
        }

        public async Task<byte[]> PostValues(string address, NameValueCollection data)
        {
            var nameValueCollection = data.AllKeys.Select(key => new KeyValuePair<string, string>(key, data.Get(key))).ToList();
            HttpResponseMessage result = await _webclient.PostAsync(address, new FormUrlEncodedContent(nameValueCollection));
            return await result.Content.ReadAsByteArrayAsync();
        }

        public async Task<string> DownloadPageAsString(string address)
        {
            HttpResponseMessage result = await _webclient.GetAsync(address);
            return await result.Content.ReadAsStringAsync();
        }

        public void Dispose()
        {
            _webclient?.Dispose();
            _webclient = null;
        }
    }
}