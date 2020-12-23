namespace FlexKids.Console.Configuration
{
    using System.Collections.Generic;

    public class NotificationSubscriptions
    {
        public EmailAddress From { get; set; }

        public List<EmailAddress> To { get; set; }
    }
}