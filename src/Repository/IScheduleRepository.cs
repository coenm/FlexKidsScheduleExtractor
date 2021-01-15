namespace Repository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Repository.Model;

    public interface IScheduleRepository
    {
        Task<IList<Schedule>> GetSchedules(int year, int week);

        Task<Schedule> Insert(Schedule schedule);

        Task<int> Delete(IEnumerable<Schedule> schedules);

        Task<Week> Insert(Week week);

        Task<Week> Update(Week originalWeek, Week updatedWeek);

        Task<Week> GetWeek(int year, int weekNr);
    }
}