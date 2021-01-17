namespace FlexKidsScheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FlexKidsScheduler.Model;
    using Repository;
    using Repository.Model;

    // A delegate type for hooking up change notifications.
    public delegate void ChangedEventHandler(object sender, ScheduleChangedEventArgs e);

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
        public event ChangedEventHandler ScheduleChanged;

        public async Task<IEnumerable<ScheduleDiff>> GetChanges()
        {
            var indexPage = await _flexKidsClient.GetAvailableSchedulesPage();
            var indexContent = _parser.GetIndexContent(indexPage);
            var somethingChanged = false;
            var weekAndHtml = new Dictionary<int, WeekAndHtml>(indexContent.Weeks.Count);

            foreach (KeyValuePair<int, WeekItem> i in indexContent.Weeks)
            {
                var htmlSchedule = await _flexKidsClient.GetSchedulePage(i.Key);
                var htmlHash = _hash.Hash(htmlSchedule);
                var week = await _repo.GetWeek(i.Value.Year, i.Value.WeekNr);

                if (week == null || htmlHash != week.Hash)
                {
                    somethingChanged = true;
                }

                weekAndHtml.Add(i.Key, new WeekAndHtml
                    {
                        Week = await GetCreateOrUpdateWeek(week, i.Value.Year, i.Value.WeekNr, htmlHash),
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

        private void OnScheduleChanged(IOrderedEnumerable<ScheduleDiff> diffs)
        {
            ScheduleChanged?.Invoke(this, new ScheduleChangedEventArgs(diffs));
        }

        private async Task<IEnumerable<ScheduleDiff>> ProcessRawData(Dictionary<int, WeekAndHtml> weekAndHtml)
        {
            var diffsResult = new List<ScheduleDiff>();

            foreach (WeekAndHtml item in weekAndHtml.Select(a => a.Value))
            {
                IList<Schedule> dbSchedules = await _repo.GetSchedules(item.Week.Year, item.Week.WeekNr);
                IList<ScheduleDiff> diffResult;
                if (item.ScheduleChanged)
                {
                    List<ScheduleItem> parsedSchedules = _parser.GetScheduleFromContent(item.Html, item.Week.Year);
                    diffResult = GetDiffs(dbSchedules, parsedSchedules, item.Week);

                    Schedule[] schedulesToDelete = diffResult
                                                   .Where(x => x.Status == ScheduleStatus.Removed)
                                                   .Select(x => x.Schedule)
                                                   .ToArray();

                    if (schedulesToDelete.Any())
                    {
                        _ = await _repo.Delete(schedulesToDelete);
                    }

                    IEnumerable<Schedule> schedulesToInsert = diffResult
                                                              .Where(x => x.Status == ScheduleStatus.Added)
                                                              .Select(x => x.Schedule);
                    foreach (Schedule schedule in schedulesToInsert)
                    {
                        _ = await _repo.Insert(schedule);
                    }

                    OnScheduleChanged(diffResult.OrderBy(x => x.Start).ThenBy(x => x.Status));
                }
                else
                {
                    diffResult = new List<ScheduleDiff>(dbSchedules.Count);
                    foreach (Schedule dbSchedule in dbSchedules)
                    {
                        diffResult.Add(new ScheduleDiff
                            {
                                Schedule = dbSchedule,
                                Status = ScheduleStatus.Unchanged,
                            });
                    }
                }

                diffsResult.AddRange(diffResult);
            }

            return diffsResult;
        }

        private IList<ScheduleDiff> GetDiffs(ICollection<Schedule> dbSchedules, ICollection<ScheduleItem> parsedSchedules, Week week)
        {
            var diffResult = new List<ScheduleDiff>(parsedSchedules.Count + dbSchedules.Count);

            foreach (Schedule item in dbSchedules)
            {
                var diffResultItem = new ScheduleDiff
                    {
                        Schedule = item,
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
                var schedule = new Schedule
                    {
                        WeekId = week.Id,
                        Week = week,
                        Location = parsedSchedule.Location,
                        StartDateTime = parsedSchedule.Start,
                        EndDateTime = parsedSchedule.End,
                    };

                diffResult.Add(new ScheduleDiff
                    {
                        Schedule = schedule,
                        Status = ScheduleStatus.Added,
                    });
            }

            return diffResult;
        }

        private async Task<Week> GetCreateOrUpdateWeek(Week week, int year, int weekNr, string htmlHash)
        {
            if (week == null)
            {
                week = await _repo.Insert(new Week
                    {
                        Hash = htmlHash,
                        Year = year,
                        WeekNr = weekNr,
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

                var newWeek = new Week
                    {
                        Hash = htmlHash,
                        WeekNr = week.WeekNr,
                        Year = week.Year,
                        Id = week.Id,
                    };

                Week w = await _repo.Update(week, newWeek);
                if (w == null)
                {
                    throw new Exception();
                }

                return w;
            }

            return week;
        }
    }
}