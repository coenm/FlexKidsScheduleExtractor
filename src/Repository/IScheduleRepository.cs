namespace Repository
{
    using System.Collections.Generic;
    using Repository.Model;

    public interface IScheduleRepository
    {
        IList<Schedule> GetSchedules(int year, int week);

        Schedule Insert(Schedule schedule);

        int Delete(IEnumerable<Schedule> schedules);

        Week Insert(Week week);

        Week Update(Week originalWeek, Week updatedWeek);

        Week GetWeek(int year, int weekNr);
    }
}