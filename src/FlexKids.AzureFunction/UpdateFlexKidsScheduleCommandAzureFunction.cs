namespace FlexKids.AzureFunction
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using FlexKids.Core.Commands;
    using FlexKids.Core.Startup;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;

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

        [FunctionName("UpdateFlexKidsSchedule")]
        public async Task<IActionResult> UpdateFlexKidsSchedule([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            await _commandHandler.ProcessAsync(new UpdateFlexKidsScheduleCommand(), default);
            return new OkResult();
        }
    }
}
