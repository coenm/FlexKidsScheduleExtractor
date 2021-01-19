namespace FlexKids.Core.Scheduler
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FlexKids.Core.Scheduler.Model;

    public interface IReportScheduleChange
    {
        Task<bool> HandleChange(IReadOnlyList<ScheduleDiff> schedule);
    }
}
