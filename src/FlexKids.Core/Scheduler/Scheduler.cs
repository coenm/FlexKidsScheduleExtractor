namespace FlexKids.Core.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FlexKids.Core.Repository;
    using FlexKids.Core.Repository.Model;
    using FlexKids.Core.Scheduler.Model;

    public class Scheduler : IDisposable
    {
        private readonly IFlexKidsClient _flexKidsClient;
        private readonly IHash _hash;
        private readonly IKseParser _parser;
        private readonly IScheduleRepository _repo;

        public Scheduler(IFlexKidsClient flexKidsClient, IKseParser parser, IScheduleRepository scheduleRepository, IHash hash)
        {
            _flexKidsClient = flexKidsClient ?? throw new ArgumentNullException(nameof(flexKidsClient));
            _hash = hash ?? throw new ArgumentNullException(nameof(hash));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _repo = scheduleRepository ?? throw new ArgumentNullException(nameof(scheduleRepository));
        }

        /// <summary>
        /// An event that clients can use to be notified whenever the elements of the list change.
        /// </summary>
        public event Func<object, ScheduleChangedEventArgs, Task> ScheduleChanged;

        public async Task<IEnumerable<ScheduleDiff>> ProcessAsync()
        {
            var indexPage = await _flexKidsClient.GetAvailableSchedulesPage();
            IndexContent indexContent = _parser.GetIndexContent(indexPage);
            var somethingChanged = false;
            var weekAndHtml = new Dictionary<int, WeekAndHtml>(indexContent.Weeks.Count);

            foreach (KeyValuePair<int, WeekItem> item in indexContent.Weeks)
            {
                var htmlSchedule = await _flexKidsClient.GetSchedulePage(item.Key);
                var htmlHash = _hash.Hash(htmlSchedule);
                var week = await _repo.GetWeek(item.Value.Year, item.Value.WeekNr);

                if (week == null || htmlHash != week.Hash)
                {
                    somethingChanged = true;
                }

                weekAndHtml.Add(item.Key, new WeekAndHtml
                    {
                        WeekSchedule = await GetCreateOrUpdateWeek(week, item.Value.Year, item.Value.WeekNr, htmlHash),
                        Hash = htmlHash,
                        Html = htmlSchedule,
                        ScheduleChanged = week == null || htmlHash != week.Hash,
                    });
            }

            if (!somethingChanged)
            {
                return Enumerable.Empty<ScheduleDiff>();
            }

            return await ProcessRawData(weekAndHtml);
        }

        public void Dispose()
        {
            // should we dispose injected instances?
        }

        private async Task OnScheduleChanged(IOrderedEnumerable<ScheduleDiff> diffs)
        {
            Func<object, ScheduleChangedEventArgs, Task> handler = ScheduleChanged;

            if (handler == null)
            {
                return;
            }

            var evt = new ScheduleChangedEventArgs(diffs);

            Delegate[] invocationList = handler.GetInvocationList();
            var handlerTasks = new Task[invocationList.Length];

            for (var i = 0; i < invocationList.Length; i++)
            {
                handlerTasks[i] = ((Func<object, ScheduleChangedEventArgs, Task>)invocationList[i])(this, evt);
            }

            await Task.WhenAll(handlerTasks);
        }

        private async Task<IEnumerable<ScheduleDiff>> ProcessRawData(Dictionary<int, WeekAndHtml> weekAndHtml)
        {
            var diffsResult = new List<ScheduleDiff>();

            foreach (WeekAndHtml item in weekAndHtml.Select(a => a.Value))
            {
                IList<SingleShift> shfits = await _repo.GetSchedules(item.WeekSchedule.Year, item.WeekSchedule.WeekNumber);
                IList<ScheduleDiff> diffResult;
                if (item.ScheduleChanged)
                {
                    List<ScheduleItem> parsedSchedules = _parser.GetScheduleFromContent(item.Html, item.WeekSchedule.Year);
                    diffResult = GetDiffs(shfits, parsedSchedules, item.WeekSchedule);

                    SingleShift[] schedulesToDelete = diffResult
                                                      .Where(x => x.Status == ScheduleStatus.Removed)
                                                      .Select(x => x.SingleShift)
                                                      .ToArray();

                    if (schedulesToDelete.Any())
                    {
                        _ = await _repo.DeleteSchedules(schedulesToDelete);
                    }

                    IEnumerable<SingleShift> schedulesToInsert = diffResult
                                                                 .Where(x => x.Status == ScheduleStatus.Added)
                                                                 .Select(x => x.SingleShift);
                    foreach (SingleShift schedule in schedulesToInsert)
                    {
                        _ = await _repo.InsertSchedule(schedule);
                    }

                    await OnScheduleChanged(diffResult.OrderBy(x => x.Start).ThenBy(x => x.Status));
                }
                else
                {
                    diffResult = new List<ScheduleDiff>(shfits.Count);
                    foreach (SingleShift singleShift in shfits)
                    {
                        diffResult.Add(new ScheduleDiff
                            {
                                SingleShift = singleShift,
                                Status = ScheduleStatus.Unchanged,
                            });
                    }
                }

                diffsResult.AddRange(diffResult);
            }

            return diffsResult;
        }

        private IList<ScheduleDiff> GetDiffs(ICollection<SingleShift> dbSchedules, ICollection<ScheduleItem> parsedSchedules, WeekSchedule week)
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
                        WeekId = week.Id,
                        WeekSchedule = week,
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

        private async Task<WeekSchedule> GetCreateOrUpdateWeek(WeekSchedule week, int year, int weekNr, string htmlHash)
        {
            if (week == null)
            {
                week = await _repo.InsertWeek(new WeekSchedule
                    {
                        Hash = htmlHash,
                        Year = year,
                        WeekNumber = weekNr,
                    });

                if (week == null)
                {
                    throw new Exception();
                }
            }
            else
            {
                if (week.Hash == htmlHash)
                {
                    return week;
                }

                week.Hash = htmlHash;

                return await _repo.UpdateWeek(week);
            }

            return week;
        }
    }
}