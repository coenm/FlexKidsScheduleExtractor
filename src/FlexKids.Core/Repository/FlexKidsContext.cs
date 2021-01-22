namespace FlexKids.Core.Repository
{
    using FlexKids.Core.Repository.Model;
    using Microsoft.EntityFrameworkCore;

    public class FlexKidsContext : DbContext
    {
        public FlexKidsContext(DbContextOptions<FlexKidsContext> options)
            : base(options)
        {
        }

        public DbSet<WeekSchedule> WeekSchedules { get; set; }

        public DbSet<SingleShift> SingleShifts { get; set; }
    }
}