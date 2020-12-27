namespace FlexKidsScheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FlexKidsScheduler.Model;

    public class ScheduleChangedArgs : EventArgs
    {
        private readonly IOrderedEnumerable<ScheduleDiff> _diff;

        public ScheduleChangedArgs(IOrderedEnumerable<ScheduleDiff> diff)
        {
            _diff = diff;
        }

        public IList<ScheduleDiff> Diff => _diff.ToList();
    }
}