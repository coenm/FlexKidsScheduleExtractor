namespace FlexKids.Core.Interfaces
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IFlexKidsClient : IDisposable
    {
        Task<string> GetSchedulePage(int id, CancellationToken cancellationToken);

        Task<string> GetAvailableSchedulesPage(CancellationToken cancellationToken);
    }
}