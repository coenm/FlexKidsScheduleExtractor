namespace Repository.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FlexKids.Core.Repository;
    using FlexKids.Core.Repository.Model;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public class EntityFrameworkScheduleRepository : IScheduleRepository
    {
        private readonly FlexKidsContext _context;

        public EntityFrameworkScheduleRepository(FlexKidsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IList<Schedule>> GetSchedules(int year, int week)
        {
            return await _context.SingleShifts
                           .Where(x => x.Week.Year == year && x.Week.WeekNr == week)
                           .ToListAsync();
        }

        public async Task<Schedule> InsertSchedule(Schedule schedule)
        {
            EntityEntry<Schedule> result = await _context.SingleShifts.AddAsync(schedule);
            _ = await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<int> DeleteSchedules(IEnumerable<Schedule> schedules)
        {
            _context.SingleShifts.RemoveRange(schedules);
            return await _context.SaveChangesAsync();
        }

        public async Task<Week> InsertWeek(Week week)
        {
            EntityEntry<Week> result = await _context.WeekSchedules.AddAsync(week);
            _ = await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<Week> UpdateWeek(Week week)
        {
            EntityEntry<Week> result = _context.WeekSchedules.Update(week);
            _ = await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<Week> GetWeek(int year, int weekNr)
        {
            return await _context.WeekSchedules.FirstOrDefaultAsync(x => x.Year == year && x.WeekNr == weekNr);
        }
    }
}