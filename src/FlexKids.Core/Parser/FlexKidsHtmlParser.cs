namespace FlexKids.Core.Parser
{
    using System.Collections.Generic;
    using FlexKids.Core.Interfaces;
    using FlexKids.Core.Scheduler;
    using FlexKids.Core.Scheduler.Model;

    public class FlexKidsHtmlParser : IKseParser
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