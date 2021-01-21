namespace FlexKids.Core.FlexKidsClient
{
    /// <summary>
    /// Configuration for <see cref="HttpFlexKidsClient"/>.
    /// </summary>
    public class FlexKidsHttpClientConfig
    {
        public FlexKidsHttpClientConfig(string hostUrl, string username, string password)
        {
            HostUrl = hostUrl;
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Gets HostUrl of FlexKids.
        /// </summary>
        public string HostUrl { get; }

        /// <summary>
        /// Gets username for logging in to FlexKids.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Gets password for logging in to FlexKids.
        /// </summary>
        public string Password { get; }
    }
}