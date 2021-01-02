namespace FlexKidsScheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FlexKidsScheduler.Model;

    public class ScheduleChangedEventArgs : EventArgs
    {
        public ScheduleChangedEventArgs(IOrderedEnumerable<ScheduleDiff> diff)
        {
            Diff = diff.ToList();
        }

        public IReadOnlyList<ScheduleDiff> Diff { get; }
    }
}