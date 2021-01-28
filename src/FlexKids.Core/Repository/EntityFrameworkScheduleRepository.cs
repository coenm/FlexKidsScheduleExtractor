namespace FlexKids.Core.Repository
{
    using System;
    using System.Threading.Tasks;
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

        public async Task<WeekSchedule> Save(WeekSchedule weekSchedule)
        {
            EntityEntry<WeekSchedule> result;

            if (weekSchedule.Id == default)
            {
                result = await _context.WeekSchedules.AddAsync(weekSchedule);
            }
            else
            {
                result = _context.WeekSchedules.Update(weekSchedule);
            }

            _ = await _context.SaveChangesAsync();

            return result.Entity;
        }

        public async Task<WeekSchedule> Get(int year, int weekNr)
        {
            WeekSchedule result = await _context.WeekSchedules
                                                .Include(schedule => schedule.Shifts)
                                                .FirstOrDefaultAsync(x => x.Year == year && x.WeekNumber == weekNr);
            if (result != null)
            {
                return result;
            }

            return new WeekSchedule
                {
                    Year = year,
                    WeekNumber = weekNr,
                };
        }
    }
}