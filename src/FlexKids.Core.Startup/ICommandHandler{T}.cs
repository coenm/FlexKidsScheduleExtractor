namespace FlexKids.Core.Startup
{
    using System.Threading;
    using System.Threading.Tasks;

    internal interface ICommandHandler<in T> : ICommandHandler
        where T : ICommand
    {
        Task HandleAsync(T command, CancellationToken ct);
    }
}