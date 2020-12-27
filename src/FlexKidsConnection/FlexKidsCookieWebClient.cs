namespace FlexKidsConnection
{
    using System.Collections.Specialized;
    using FlexKidsScheduler;

    public class FlexKidsCookieWebClient : IFlexKidsConnection
    {
        private readonly IWeb _web;
        private readonly FlexKidsCookieConfig _config;
        private bool _isLoggedIn;

        public FlexKidsCookieWebClient(IWeb web, FlexKidsCookieConfig config)
        {
            _web = web;
            _config = config;
        }

        public string GetSchedulePage(int id)
        {
            if (!_isLoggedIn)
            {
                Login();
            }

            var urlSchedule = string.Format(_config.HostUrl + "/personeel/rooster/week?week={0}", id);
            return _web.DownloadPageAsString(urlSchedule);
        }

        public string GetAvailableSchedulesPage()
        {
            if (!_isLoggedIn)
            {
                Login();
            }

            return _web.DownloadPageAsString(_config.HostUrl + "/personeel/rooster/index");
        }

        public void Dispose()
        {
            _web?.Dispose();
        }

        private void Login()
        {
            var requestParams = new NameValueCollection
                {
                    { "username", _config.Username },
                    { "password", _config.Password },
                    { "role", "4" },
                    { "login", "Log in" },
                };

            _ = _web.PostValues(_config.HostUrl + "/user/process", requestParams);

            _isLoggedIn = true;
        }
    }
}
