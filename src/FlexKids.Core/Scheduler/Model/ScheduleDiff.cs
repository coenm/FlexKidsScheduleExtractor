namespace FlexKids.Core.Scheduler.Model
{
    using System;
    using FlexKids.Core.Repository.Model;

    public struct ScheduleDiff
    {
        public ScheduleStatus Status { get; set; }

        public DateTime Start => SingleShift.StartDateTime;

        public SingleShift SingleShift { get; set; }
    }
}