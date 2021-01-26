namespace FlexKids.AzureFunction
{
    using System.Threading;
    using System.Threading.Tasks;
    using FlexKids.Core.Startup;
    using Microsoft.Azure.WebJobs;

    public class UpdateFlexKidsScheduleCommandAzureFunction
    {
        private readonly Executor _commandHandler;

        public UpdateFlexKidsScheduleCommandAzureFunction(Executor commandHandler)
        {
            _commandHandler = commandHandler;
        }

        [FunctionName("Function1")]
        public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer)
        {
            await _commandHandler.ProcessAsync(new UpdateFlexKidsScheduleCommand(), CancellationToken.None);
        }
    }
}
