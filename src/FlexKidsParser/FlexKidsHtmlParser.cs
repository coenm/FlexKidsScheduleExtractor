namespace FlexKidsParser
{
    using System.Collections.Generic;
    using FlexKidsParser.Model;

    public class FlexKidsHtmlParser
    {
        public IndexContent GetIndexContent(string html)
        {
            var parser = new IndexParser(html);
            return parser.Parse();
        }

        public List<ScheduleItem> GetScheduleFromContent(string html, int year)
        {
            var parser = new ScheduleParser(html, year);
            return parser.GetScheduleFromContent();
        }
    }
}