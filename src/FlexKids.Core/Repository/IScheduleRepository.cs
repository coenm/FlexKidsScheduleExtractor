namespace FlexKids.Core.Repository
{
    using System.Threading.Tasks;
    using FlexKids.Core.Repository.Model;

    public interface IScheduleRepository
    {
        Task<WeekSchedule> Get(int year, int weekNr);

        Task<WeekSchedule> Save(WeekSchedule weekSchedule);
    }
}