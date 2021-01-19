namespace Repository.EntityFramework
{
    using FlexKids.Core.Repository.Model;
    using Microsoft.EntityFrameworkCore;

    public class FlexKidsContext : DbContext
    {
        public FlexKidsContext(DbContextOptions<FlexKidsContext> options)
            : base(options)
        {
        }

        public DbSet<Week> WeekSchedules { get; set; }

        public DbSet<Schedule> SingleShifts { get; set; }
    }
}