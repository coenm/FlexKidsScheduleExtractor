namespace FlexKidsScheduler
{
    using System.Collections.Generic;
    using FlexKidsScheduler.Model;

    public interface IKseParser
    {
        IndexContent GetIndexContent(string html);

        List<ScheduleItem> GetScheduleFromContent(string html, int year);
    }
}