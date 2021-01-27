namespace FlexKids.AzureFunction
{
    using System;
    using System.Threading.Tasks;
    using FlexKids.Core.Commands;
    using FlexKids.Core.Startup;
    using Microsoft.Azure.WebJobs;

    public class UpdateFlexKidsScheduleCommandAzureFunction
    {
        private readonly Executor _commandHandler;

        public UpdateFlexKidsScheduleCommandAzureFunction(Executor commandHandler)
        {
            _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        }

        [FunctionName("UpdateFlexKidsScheduleTimer")]
        public async Task UpdateFlexKidsScheduleTimer([TimerTrigger("%FrequencyUpdateFlexKidsScheduleTimer%")]TimerInfo myTimer)
        {
            var cmd = new UpdateFlexKidsScheduleCommand();
            await _commandHandler.ProcessAsync(cmd, default);
        }
    }
}
