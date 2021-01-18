namespace FlexKidsScheduler
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FlexKidsScheduler.Model;

    public interface IReportScheduleChange
    {
        Task<bool> HandleChange(IReadOnlyList<ScheduleDiff> schedule);
    }
}
