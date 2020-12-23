namespace FlexKids.Console
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net.Http;
    using FlexKidsConnection;

    public class WebClientAdapter : IWeb
    {
        private HttpClient _webclient;

        public WebClientAdapter()
        {
            _webclient = new HttpClient();
        }

        public byte[] PostValues(string address, NameValueCollection data)
        {
            var nameValueCollection = data.AllKeys.Select(key => new KeyValuePair<string, string>(key, data.Get(key))).ToList();

            return _webclient.PostAsync(address, new FormUrlEncodedContent(nameValueCollection))
                            .GetAwaiter()
                            .GetResult()
                            .Content
                            .ReadAsByteArrayAsync()
                            .GetAwaiter()
                            .GetResult();
        }

        public string DownloadPageAsString(string address)
        {
            return _webclient.GetAsync(address)
                            .GetAwaiter()
                            .GetResult()
                            .Content
                            .ReadAsStringAsync()
                            .GetAwaiter()
                            .GetResult();
        }

        public void Dispose()
        {
            _webclient?.Dispose();
            _webclient = null;
        }
    }
}