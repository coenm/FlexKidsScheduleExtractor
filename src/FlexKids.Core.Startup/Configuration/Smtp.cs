namespace FlexKids.Core.Startup.Configuration
{
    public class Smtp
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public bool Secure { get; set; }
    }
}