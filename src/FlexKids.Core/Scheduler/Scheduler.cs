namespace FlexKids.Core.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FlexKids.Core.Interfaces;
    using FlexKids.Core.Repository;
    using FlexKids.Core.Repository.Model;
    using FlexKids.Core.Scheduler.Model;

    public class Scheduler : IDisposable
    {
        private readonly IFlexKidsClient _flexKidsClient;
        private readonly IKseParser _parser;
        private readonly IScheduleRepository _repo;

        public Scheduler(IFlexKidsClient flexKidsClient, IKseParser parser, IScheduleRepository scheduleRepository)
        {
            _flexKidsClient = flexKidsClient ?? throw new ArgumentNullException(nameof(flexKidsClient));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _repo = scheduleRepository ?? throw new ArgumentNullException(nameof(scheduleRepository));
        }

        /// <summary>
        /// An event that clients can use to be notified whenever the elements of the list change.
        /// </summary>
        public event Func<object, ScheduleChangedEventArgs, Task> ScheduleChanged;

        public async Task<IEnumerable<ScheduleDiff>> ProcessAsync(CancellationToken ct = default)
        {
            var indexPage = await _flexKidsClient.GetAvailableSchedulesPage(ct);
            IndexContent indexContent = _parser.GetIndexContent(indexPage);
            var weekSchedulesToImport = new List<WeekAndImportedSchedules>(indexContent.Weeks.Count);

            foreach (KeyValuePair<int, WeekItem> item in indexContent.Weeks)
            {
                var htmlComboboxIndex = item.Key;
                var year = item.Value.Year;
                var weekNumber = item.Value.WeekNr;

                var htmlSchedule = await _flexKidsClient.GetSchedulePage(htmlComboboxIndex, ct);
                var parsedSchedules = _parser.GetScheduleFromContent(htmlSchedule, year).ToList();

                WeekSchedule weekSchedule = await _repo.Get(year, weekNumber);

                weekSchedulesToImport.Add(new WeekAndImportedSchedules
                    {
                        WeekSchedule = weekSchedule,
                        ScheduleItems = parsedSchedules,
                    });
            }

            return await ProcessRawData(weekSchedulesToImport);
        }

        public void Dispose()
        {
            // should we dispose injected instances?
        }

        private async Task OnScheduleChanged(WeekSchedule updatedWeekSchedule, IOrderedEnumerable<ScheduleDiff> diffs)
        {
            Func<object, ScheduleChangedEventArgs, Task> handler = ScheduleChanged;

            if (handler == null)
            {
                return;
            }

            var evt = new ScheduleChangedEventArgs(updatedWeekSchedule, diffs);

            Delegate[] invocationList = handler.GetInvocationList();
            var handlerTasks = new Task[invocationList.Length];

            for (var i = 0; i < invocationList.Length; i++)
            {
                handlerTasks[i] = ((Func<object, ScheduleChangedEventArgs, Task>)invocationList[i])(this, evt);
            }

            await Task.WhenAll(handlerTasks);
        }

        private async Task<IEnumerable<ScheduleDiff>> ProcessRawData(IEnumerable<WeekAndImportedSchedules> weekAndHtml)
        {
            var diffsResult = new List<ScheduleDiff>();

            foreach (WeekAndImportedSchedules item in weekAndHtml)
            {
                var scheduleChanged = false;

                IList<SingleShift> shiftsInRepository = item.WeekSchedule.Shifts;
                IList<ScheduleItem> parsedSchedules = item.ScheduleItems;
                IList<ScheduleDiff> diffResult = GetDiffs(shiftsInRepository, parsedSchedules);

                foreach (SingleShift shift in diffResult.Where(x => x.Status == ScheduleStatus.Removed).Select(x => x.SingleShift))
                {
                    _ = item.WeekSchedule.Shifts.Remove(shift);
                    scheduleChanged = true;
                }

                foreach (SingleShift shift in diffResult.Where(x => x.Status == ScheduleStatus.Added).Select(x => x.SingleShift))
                {
                    item.WeekSchedule.Shifts.Add(shift);
                    scheduleChanged = true;
                }

                if (scheduleChanged)
                {
                    WeekSchedule updatedWeekSchedule = await _repo.Save(item.WeekSchedule);
                    await OnScheduleChanged(
                        updatedWeekSchedule,
                        diffResult.OrderBy(x => x.Start).ThenBy(x => x.Status));
                }

                diffsResult.AddRange(diffResult);
            }

            return diffsResult;
        }

        private IList<ScheduleDiff> GetDiffs(ICollection<SingleShift> dbSchedules, ICollection<ScheduleItem> parsedSchedules)
        {
            var diffResult = new List<ScheduleDiff>(parsedSchedules.Count + dbSchedules.Count);

            foreach (SingleShift item in dbSchedules)
            {
                var diffResultItem = new ScheduleDiff
                    {
                        SingleShift = item,
                    };

                ScheduleItem selectItem = parsedSchedules.FirstOrDefault(scheduleItem =>
                    scheduleItem.Start == item.StartDateTime
                    &&
                    scheduleItem.End == item.EndDateTime
                    &&
                    scheduleItem.Location == item.Location);

                if (selectItem != null)
                {
                    diffResultItem.Status = ScheduleStatus.Unchanged;
                    _ = parsedSchedules.Remove(selectItem);
                }
                else
                {
                    diffResultItem.Status = ScheduleStatus.Removed;
                }

                diffResult.Add(diffResultItem);
            }

            foreach (ScheduleItem parsedSchedule in parsedSchedules)
            {
                var schedule = new SingleShift
                    {
                        Location = parsedSchedule.Location,
                        StartDateTime = parsedSchedule.Start,
                        EndDateTime = parsedSchedule.End,
                    };

                diffResult.Add(new ScheduleDiff
                    {
                        SingleShift = schedule,
                        Status = ScheduleStatus.Added,
                    });
            }

            return diffResult;
        }
    }
}