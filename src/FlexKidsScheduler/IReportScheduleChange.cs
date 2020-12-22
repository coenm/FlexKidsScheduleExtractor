namespace FlexKidsScheduler
{
    using System.Collections.Generic;
    using FlexKidsScheduler.Model;

    public interface IReportScheduleChange
    {
        bool HandleChange(IList<ScheduleDiff> schedule);
    }
}
