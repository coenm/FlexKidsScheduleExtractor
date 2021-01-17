namespace Reporter.Email
{
    public class EmailConfig
    {
        public EmailConfig(string emailFrom, string emailTo2, string emailToName2, string emailTo1, string emailToName1)
        {
            EmailFrom = emailFrom;
            EmailTo2 = emailTo2;
            EmailToName2 = emailToName2;
            EmailTo1 = emailTo1;
            EmailToName1 = emailToName1;
        }

        public string EmailFrom { get; }

        public string EmailTo2 { get; }

        public string EmailToName2 { get; }

        public string EmailTo1 { get; }

        public string EmailToName1 { get; }
    }
}