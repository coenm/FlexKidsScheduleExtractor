namespace FlexKids.AzureFunction
{
    using System;
    using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Contains required attribute")]
        public async Task UpdateFlexKidsScheduleTimer([TimerTrigger("%FrequencyUpdateFlexKidsScheduleTimer%")]TimerInfo timer)
        {
            var cmd = new UpdateFlexKidsScheduleCommand();
            await _commandHandler.ProcessAsync(cmd, default);
        }
    }
}
