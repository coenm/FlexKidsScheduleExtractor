namespace FlexKidsParser.Model
{
    using System.Collections.Generic;

    public class IndexContent
    {
        public bool IsLoggedin { get; set; }

        public string Email { get; set; }

        public Dictionary<int, WeekItem> Weeks { get; set; }
    }
}