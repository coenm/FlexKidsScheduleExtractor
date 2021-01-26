namespace FlexKids.Core.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FlexKids.Core.Repository.Model;
    using FlexKids.Core.Scheduler.Model;

    public interface IReportScheduleChange
    {
        Task<bool> HandleChange(IReadOnlyList<ScheduleDiff> schedule, WeekSchedule updatedWeekSchedule);
    }
}
