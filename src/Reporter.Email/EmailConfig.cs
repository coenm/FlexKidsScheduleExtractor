namespace Reporter.Email
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;

    public class EmailConfig
    {
        public EmailConfig(MailAddress emailFrom, params MailAddress[] to)
        {
            EmailFrom = emailFrom ?? throw new ArgumentNullException(nameof(emailFrom));
            To = to?.ToList() ?? new List<MailAddress>(0);

            if (To.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(to));
            }
        }

        public MailAddress EmailFrom { get; }

        public IReadOnlyList<MailAddress> To { get; }
    }
}