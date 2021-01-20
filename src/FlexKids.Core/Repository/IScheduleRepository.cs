namespace FlexKids.Core.Repository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FlexKids.Core.Repository.Model;

    public interface IScheduleRepository
    {
        Task<IList<SingleShift>> GetSchedules(int year, int week);

        Task<SingleShift> InsertSchedule(SingleShift singleShift);

        Task<int> DeleteSchedules(IEnumerable<SingleShift> schedules);

        Task<WeekSchedule> InsertWeek(WeekSchedule weekSchedule);

        Task<WeekSchedule> UpdateWeek(WeekSchedule weekSchedule);

        Task<WeekSchedule> GetWeek(int year, int weekNr);
    }
}