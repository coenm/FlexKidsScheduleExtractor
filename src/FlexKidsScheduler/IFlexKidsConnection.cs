namespace FlexKidsScheduler
{
    using System;
    using System.Threading.Tasks;

    public interface IFlexKidsConnection : IDisposable
    {
        Task<string> GetSchedulePage(int id);

        Task<string> GetAvailableSchedulesPage();
    }
}