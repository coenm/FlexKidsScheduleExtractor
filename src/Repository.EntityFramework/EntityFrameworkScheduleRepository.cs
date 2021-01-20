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

        public async Task<IList<SingleShift>> GetSchedules(int year, int week)
        {
            return await _context.SingleShifts
                           .Where(x => x.WeekSchedule.Year == year && x.WeekSchedule.WeekNumber == week)
                           .ToListAsync();
        }

        public async Task<SingleShift> InsertSchedule(SingleShift singleShift)
        {
            EntityEntry<SingleShift> result = await _context.SingleShifts.AddAsync(singleShift);
            _ = await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<int> DeleteSchedules(IEnumerable<SingleShift> schedules)
        {
            _context.SingleShifts.RemoveRange(schedules);
            return await _context.SaveChangesAsync();
        }

        public async Task<WeekSchedule> InsertWeek(WeekSchedule weekSchedule)
        {
            EntityEntry<WeekSchedule> result = await _context.WeekSchedules.AddAsync(weekSchedule);
            _ = await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<WeekSchedule> UpdateWeek(WeekSchedule weekSchedule)
        {
            EntityEntry<WeekSchedule> result = _context.WeekSchedules.Update(weekSchedule);
            _ = await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<WeekSchedule> GetWeek(int year, int weekNr)
        {
            return await _context.WeekSchedules.FirstOrDefaultAsync(x => x.Year == year && x.WeekNumber == weekNr);
        }
    }
}