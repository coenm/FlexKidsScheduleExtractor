namespace Repository.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Repository.Model;

    public class EntityFrameworkScheduleRepository : IScheduleRepository
    {
        private readonly FlexKidsContext _context;

        public EntityFrameworkScheduleRepository(FlexKidsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        async Task<IList<Schedule>> IScheduleRepository.GetSchedules(int year, int week)
        {
            return await _context.SingleShifts
                           .Where(x => x.Week.Year == year && x.Week.WeekNr == week)
                           .ToListAsync();
        }

        public async Task<Schedule> Insert(Schedule schedule)
        {
            EntityEntry<Schedule> result = await _context.SingleShifts.AddAsync(schedule);
            _ = await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<int> Delete(IEnumerable<Schedule> schedules)
        {
            _context.SingleShifts.RemoveRange(schedules);
            return await _context.SaveChangesAsync();
        }

        public async Task<Week> Insert(Week week)
        {
            EntityEntry<Week> result = await _context.WeekSchedules.AddAsync(week);
            _ = await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<Week> Update(Week originalWeek, Week updatedWeek)
        {
            EntityEntry<Week> result = _context.WeekSchedules.Update(updatedWeek);
            _ = await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<Week> GetWeek(int year, int weekNr)
        {
            return await _context.WeekSchedules.FirstOrDefaultAsync(x => x.Year == year && x.WeekNr == weekNr);
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