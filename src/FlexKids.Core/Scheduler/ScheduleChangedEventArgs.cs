namespace FlexKids.Core.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FlexKids.Core.Repository.Model;
    using FlexKids.Core.Scheduler.Model;

    public class ScheduleChangedEventArgs : EventArgs
    {
        public ScheduleChangedEventArgs(
            WeekSchedule updatedWeekSchedule,
            IOrderedEnumerable<ScheduleDiff> diff)
        {
            UpdatedWeekSchedule = updatedWeekSchedule;
            Diff = diff.ToList();
        }

        public IReadOnlyList<ScheduleDiff> Diff { get; }

        public WeekSchedule UpdatedWeekSchedule { get; }
    }
}