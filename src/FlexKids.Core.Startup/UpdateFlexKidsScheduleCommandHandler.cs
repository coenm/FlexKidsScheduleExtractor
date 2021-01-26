namespace FlexKids.Core.Startup
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FlexKids.Core.Interfaces;
    using FlexKids.Core.Scheduler;
    using Microsoft.Extensions.Logging;

    internal class UpdateFlexKidsScheduleCommandHandler : ICommandHandler<UpdateFlexKidsScheduleCommand>
    {
        private readonly Scheduler _scheduler;
        private readonly IEnumerable<IReportScheduleChange> _changedHandlers;
        private readonly ILogger _logger;

        public UpdateFlexKidsScheduleCommandHandler(Scheduler scheduler, IEnumerable<IReportScheduleChange> changedHandlers, ILogger logger)
        {
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _changedHandlers = changedHandlers ?? throw new ArgumentNullException(nameof(changedHandlers));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        Task ICommandHandler.HandleAsync(ICommand command, CancellationToken ct)
        {
            return HandleAsync(command as UpdateFlexKidsScheduleCommand, ct);
        }

        public async Task HandleAsync(UpdateFlexKidsScheduleCommand command, CancellationToken ct)
        {
            async Task DelegateScheduleChangedToReporters(object sender, ScheduleChangedEventArgs changedArgs)
            {
                foreach (IReportScheduleChange handler in _changedHandlers)
                {
                    var handlerType = handler.GetType().Name;
                    try
                    {
                        _logger.LogInformation($"Start handling using {handlerType}");
                        _ = await handler.HandleChange(changedArgs.Diff, changedArgs.UpdatedWeekSchedule);
                        _logger.LogInformation($"Done handling using {handlerType}");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Handling using {handlerType} failed.");
                    }
                }
            }

            _scheduler.ScheduleChanged += DelegateScheduleChangedToReporters;
            _logger.LogInformation("Start scheduler");
            try
            {
                _ = await _scheduler.ProcessAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                _logger.LogInformation("Finished scheduler");
                _scheduler.ScheduleChanged -= DelegateScheduleChangedToReporters;
            }
        }
    }
}