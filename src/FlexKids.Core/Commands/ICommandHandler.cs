namespace FlexKids.Core.Commands
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICommandHandler
    {
        Task HandleAsync(ICommand command, CancellationToken ct);
    }
}