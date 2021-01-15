namespace Repository.EntityFramework
{
    using Microsoft.EntityFrameworkCore;
    using Repository.Model;

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