namespace FlexKids.Core.Commands
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICommandHandler<in T> : ICommandHandler
        where T : ICommand
    {
        Task HandleAsync(T command, CancellationToken ct);
    }
}