namespace FlexKids.Core.Repository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FlexKids.Core.Repository.Model;

    public interface IScheduleRepository
    {
        Task<IList<Schedule>> GetSchedules(int year, int week);

        Task<Schedule> InsertSchedule(Schedule schedule);

        Task<int> DeleteSchedules(IEnumerable<Schedule> schedules);

        Task<Week> InsertWeek(Week week);

        Task<Week> UpdateWeek(Week week);

        Task<Week> GetWeek(int year, int weekNr);
    }
}