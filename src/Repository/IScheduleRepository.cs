namespace Repository
{
    using System;
    using System.Collections.Generic;
    using Repository.Model;

    public interface IScheduleRepository
    {
        IList<Schedule> GetSchedules(int year, int week);

        IList<Schedule> GetSchedules(DateTime from, DateTime until);

        Schedule GetSchedule(int id);

        Schedule Insert(Schedule schedule);

        Schedule Update(Schedule originalSchedule, Schedule updatedSchedule);

        int Delete(IEnumerable<Schedule> schedules);

        Week Insert(Week week);

        Week Update(Week originalWeek, Week updatedWeek);

        Week GetWeek(int year, int weekNr);

        Week GetWeek(int weekId);
    }
}