namespace FlexKidsConnection
{
    public class FlexKidsHttpClientConfig
    {
        public FlexKidsHttpClientConfig(string hostUrl, string username, string password)
        {
            HostUrl = hostUrl;
            Username = username;
            Password = password;
        }

        public string HostUrl { get; }

        public string Username { get; }

        public string Password { get; }
    }
}