namespace Repository.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Repository.Model;

    public class EntityFrameworkScheduleRepository : IScheduleRepository
    {
        private readonly FlexKidsContext _context;

        public EntityFrameworkScheduleRepository(FlexKidsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        IList<Schedule> IScheduleRepository.GetSchedules(int year, int week)
        {
            return _context.SingleShifts
                           .Where(x => x.Week.Year == year && x.Week.WeekNr == week)
                           .ToList();
        }

        public Schedule Insert(Schedule schedule)
        {
            EntityEntry<Schedule> result = _context.SingleShifts.Add(schedule);
            _ = _context.SaveChanges();
            return result.Entity;
        }

        public int Delete(IEnumerable<Schedule> schedules)
        {
            _context.SingleShifts.RemoveRange(schedules);
            return _context.SaveChanges();
        }

        public Week Insert(Week week)
        {
            EntityEntry<Week> result = _context.WeekSchedules.Add(week);
            _ = _context.SaveChanges();
            return result.Entity;
        }

        public Week Update(Week originalWeek, Week updatedWeek)
        {
            EntityEntry<Week> result = _context.WeekSchedules.Update(updatedWeek);
            _ = _context.SaveChanges();
            return result.Entity;
        }

        public Week GetWeek(int year, int weekNr)
        {
            return _context.WeekSchedules.FirstOrDefault(x => x.Year == year && x.WeekNr == weekNr);
        }

        // public async Task<IList<WeekSchedule>> GetSchedules(int year, int week)
        // {
        //     return await _context.WeekSchedules
        //                                .Where(x => x.Year == year && x.WeekNr == week)
        //                                .ToListAsync();
        // }
        //
        // public async Task<WeekSchedule> Insert(WeekSchedule schedule)
        // {
        //     EntityEntry<WeekSchedule> entity = await _context.WeekSchedules.AddAsync(schedule);
        //     _ = await _context.SaveChangesAsync();
        //     return entity.Entity;
        // }
        //
        // public async Task<WeekSchedule> Update(WeekSchedule schedule)
        // {
        //     EntityEntry<WeekSchedule> entity = _context.WeekSchedules.Update(schedule);
        //     _ = await _context.SaveChangesAsync();
        //     return entity.Entity;
        // }
        //
        // public async Task<int> Delete(WeekSchedule schedule)
        // {
        //     _ = _context.WeekSchedules.Remove(schedule);
        //     return await _context.SaveChangesAsync();
        // }
    }
}