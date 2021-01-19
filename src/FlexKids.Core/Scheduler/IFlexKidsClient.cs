namespace FlexKids.Core.Scheduler
{
    using System;
    using System.Threading.Tasks;

    public interface IFlexKidsClient : IDisposable
    {
        Task<string> GetSchedulePage(int id);

        Task<string> GetAvailableSchedulesPage();
    }
}