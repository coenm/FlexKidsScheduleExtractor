namespace FlexKids.AzureFunction
{
    using System.Threading;
    using System.Threading.Tasks;
    using FlexKids.Core.Startup;
    using Google.Apis.Util;
    using Microsoft.Azure.WebJobs;

    public class Function1
    {
        private readonly Executor _commandHandler;

        public Function1(Executor commandHandler)
        {
            _commandHandler = commandHandler;
        }

        [FunctionName("Function1")]
        public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer)
        {
            await _commandHandler.ProcessAsync(new UpdateFlexKidsScheduleCommand(), CancellationToken.None);
            await _commandHandler.Main();
        }
    }
}
