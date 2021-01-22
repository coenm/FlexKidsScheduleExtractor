namespace FlexKids.Core.Scheduler.Model
{
    using System.Collections.Generic;

    public class IndexContent
    {
        public bool IsLoggedIn => !string.IsNullOrWhiteSpace(Email);

        public string Email { get; set; }

        public Dictionary<int, WeekItem> Weeks { get; set; }
    }
}