namespace Repository
{
    public interface IScheduleRepositoryFactory
    {
        IScheduleRepository CreateScheduleRepository();
    }
}