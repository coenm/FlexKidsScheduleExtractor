namespace FlexKids.Core.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FlexKids.Core.Scheduler.Model;

    public class ScheduleChangedEventArgs : EventArgs
    {
        public ScheduleChangedEventArgs(IOrderedEnumerable<ScheduleDiff> diff)
        {
            Diff = diff.ToList();
        }

        public IReadOnlyList<ScheduleDiff> Diff { get; }
    }
}