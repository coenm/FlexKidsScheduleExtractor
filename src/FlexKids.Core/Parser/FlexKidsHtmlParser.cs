namespace FlexKids.Core.Parser
{
    using System.Collections.Generic;
    using FlexKids.Core.Interfaces;
    using FlexKids.Core.Scheduler.Model;
    using Microsoft.Extensions.Logging.Abstractions;

    public class FlexKidsHtmlParser : IKseParser
    {
        public IndexContent GetIndexContent(string html)
        {
            var parser = new IndexParser(NullLogger.Instance);
            return parser.Parse(html);
        }

        public IReadOnlyList<ScheduleItem> GetScheduleFromContent(string html, int year)
        {
            var parser = new ScheduleParser(NullLogger.Instance);
            return parser.GetScheduleFromContent(html, year);
        }
    }
}