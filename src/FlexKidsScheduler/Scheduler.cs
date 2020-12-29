namespace FlexKidsScheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FlexKidsScheduler.Model;
    using Repository;
    using Repository.Model;

    // A delegate type for hooking up change notifications.
    public delegate void ChangedEventHandler(object sender, ScheduleChangedArgs e);

    public class Scheduler : IDisposable
    {
        private readonly IFlexKidsConnection _flexKidsConnection;
        private readonly IHash _hash;
        private readonly IKseParser _parser;
        private readonly IScheduleRepository _repo;

        public Scheduler(IFlexKidsConnection flexKidsConnection, IKseParser parser, IScheduleRepository scheduleRepository, IHash hash)
        {
            _flexKidsConnection = flexKidsConnection;
            _hash = hash;
            _parser = parser;
            _repo = scheduleRepository;
        }

        // An event that clients can use to be notified whenever the
        // elements of the list change.
        public event ChangedEventHandler ScheduleChanged;

        public IEnumerable<ScheduleDiff> GetChanges()
        {
            var rooterFirstPage = _flexKidsConnection.GetAvailableSchedulesPage();
            var indexContent = _parser.GetIndexContent(rooterFirstPage);
            var somethingChanged = false;
            var weekAndHtml = new Dictionary<int, WeekAndHtml>(indexContent.Weeks.Count);

            foreach (var i in indexContent.Weeks)
            {
                var htmlSchedule = _flexKidsConnection.GetSchedulePage(i.Key);
                var htmlHash = _hash.Hash(htmlSchedule);
                var week = _repo.GetWeek(i.Value.Year, i.Value.WeekNr);

                if (week == null || htmlHash != week.Hash)
                {
                    somethingChanged = true;
                }

                weekAndHtml.Add(i.Key, new WeekAndHtml
                {
                    Week = GetCreateOrUpdateWeek(week, i.Value.Year, i.Value.WeekNr, htmlHash),
                    Hash = htmlHash,
                    Html = htmlSchedule,
                    ScheduleChanged = week == null || htmlHash != week.Hash,
                });
            }

            if (!somethingChanged)
            {
                return Enumerable.Empty<ScheduleDiff>();
            }

            var diffsResult = new List<ScheduleDiff>();

            foreach (var item in weekAndHtml.Select(a => a.Value))
            {
                var dbSchedules = _repo.GetSchedules(item.Week.Year, item.Week.WeekNr);
                IList<ScheduleDiff> diffResult;
                if (item.ScheduleChanged)
                {
                    var parsedSchedules = _parser.GetScheduleFromContent(item.Html, item.Week.Year);
                    diffResult = GetDiffs(dbSchedules, parsedSchedules, item.Week);

                    var schedulesToDelete = diffResult
                        .Where(x => x.Status == ScheduleStatus.Removed)
                        .Select(x => x.Schedule);
                    _ = _repo.Delete(schedulesToDelete);

                    var schedulesToInsert = diffResult
                        .Where(x => x.Status == ScheduleStatus.Added)
                        .Select(x => x.Schedule);
                    foreach (var schedule in schedulesToInsert)
                    {
                        _ = _repo.Insert(schedule);
                    }

                    OnScheduleChanged(diffResult.OrderBy(x => x.Start).ThenBy(x => x.Status));
                }
                else
                {
                    diffResult = new List<ScheduleDiff>(dbSchedules.Count);
                    foreach (var dbSchedule in dbSchedules)
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

        public void Dispose()
        {
            // should we dispose injected instances?
        }

        protected virtual void OnScheduleChanged(IOrderedEnumerable<ScheduleDiff> diffs)
        {
            ScheduleChanged?.Invoke(this, new ScheduleChangedArgs(diffs));
        }

        private static IList<ScheduleDiff> GetDiffs(ICollection<Schedule> dbSchedules, ICollection<ScheduleItem> parsedSchedules, Week week)
        {
            var diffResult = new List<ScheduleDiff>(parsedSchedules.Count + dbSchedules.Count);

            foreach (var item in dbSchedules)
            {
                var diffResultItem = new ScheduleDiff
                    {
                        Schedule = item,
                    };

                var selectItem = parsedSchedules.FirstOrDefault(x => x.Start == item.StartDateTime && x.End == item.EndDateTime && x.Location == item.Location);

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

            foreach (var parsedSchedule in parsedSchedules)
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

        private Week GetCreateOrUpdateWeek(Week week, int year, int weekNr, string htmlHash)
        {
            if (week == null)
            {
                week = _repo.Insert(new Week { Hash = htmlHash, Year = year, WeekNr = weekNr });
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

                // week.Hash = htmlHash;
                var w = _repo.Update(week, newWeek);
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