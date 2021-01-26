namespace FlexKids.Core.Startup
{
    using System.Threading;
    using System.Threading.Tasks;

    internal interface ICommandHandler
    {
        Task HandleAsync(ICommand command, CancellationToken ct);
    }
}