namespace FlexKidsScheduler
{
    using System;
    using System.Threading.Tasks;

    public interface IFlexKidsClient : IDisposable
    {
        Task<string> GetSchedulePage(int id);

        Task<string> GetAvailableSchedulesPage();
    }
}