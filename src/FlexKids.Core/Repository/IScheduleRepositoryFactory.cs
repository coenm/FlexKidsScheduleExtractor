namespace FlexKids.Core.Repository
{
    public interface IScheduleRepositoryFactory
    {
        IScheduleRepository CreateScheduleRepository();
    }
}