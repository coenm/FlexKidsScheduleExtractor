namespace FlexKids.Core.Scheduler
{
    using System.Collections.Generic;
    using FlexKids.Core.Scheduler.Model;

    public interface IKseParser
    {
        IndexContent GetIndexContent(string html);

        List<ScheduleItem> GetScheduleFromContent(string html, int year);
    }
}