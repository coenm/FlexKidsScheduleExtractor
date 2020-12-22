namespace FlexKidsScheduler
{
    using System;

    public class FlexKidsConfig : IFlexKidsConfig
    {
        public static readonly IFlexKidsConfig Instance = new FlexKidsConfig();

        private FlexKidsConfig()
        {
        }

        public string EmailFrom => GetConfigProperty<string>("EmailFrom");

        public string EmailTo2 => GetConfigProperty<string>("EmailTo2");

        public string EmailToName2 => GetConfigProperty<string>("EmailToName2");

        public string EmailTo1 => GetConfigProperty<string>("EmailTo1");

        public string EmailToName1 => GetConfigProperty<string>("EmailToName1");

        public string SmtpHost => GetConfigProperty<string>("SmtpHost");

        public int SmtpPort => GetConfigProperty<int>("SmtpPort");

        public string SmtpUsername => GetConfigProperty<string>("SmtpUsername");

        public string SmtpPassword => GetConfigProperty<string>("SmtpPassword");

        public string GoogleCalendarAccount => GetConfigProperty<string>("GoogleCalendarAccount");

        public string GoogleCalendarId => GetConfigProperty<string>("GoogleCalendarId");

        public string GoogleCalendarKeyFile => GetConfigProperty<string>("GoogleCalendarKeyFile");

        private static T GetConfigProperty<T>(string name) where T : IConvertible
        {
            // if (ConfigurationManager.AppSettings[name] != null)
            // {
            //     var s = ConfigurationManager.AppSettings[name].Trim();
            //     return (T)Convert.ChangeType(s, typeof(T));
            // }
            //
            // throw new ConfigurationErrorsException(String.Format("Cannot find config setting {0}", name));

            return default;
        }
    }
}
