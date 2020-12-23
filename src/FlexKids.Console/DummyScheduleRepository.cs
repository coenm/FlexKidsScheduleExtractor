namespace FlexKids.Console
{
    using System;
    using System.Collections.Generic;
    using Repository;
    using Repository.Model;

    public class DummyScheduleRepository : IScheduleRepository
    {
        public IList<Schedule> GetSchedules(int year, int week)
        {
            return new List<Schedule>(0);
        }

        public IList<Schedule> GetSchedules(DateTime @from, DateTime until)
        {
            return new List<Schedule>(0);
        }

        public Schedule GetSchedule(int id)
        {
            return new Schedule();
        }

        public Schedule Insert(Schedule schedule)
        {
            return schedule;
        }

        public Schedule Update(Schedule originalSchedule, Schedule updatedSchedule)
        {
            return updatedSchedule;
        }

        public int Delete(IEnumerable<Schedule> schedules)
        {
            return 0;
        }

        public Week Insert(Week week)
        {
            return new Week();
        }

        public Week Update(Week originalWeek, Week updatedWeek)
        {
            return new Week();
        }

        public Week GetWeek(int year, int weekNr)
        {
            return new Week();
        }

        public Week GetWeek(int weekId)
        {
            return new Week();
        }
    }
}