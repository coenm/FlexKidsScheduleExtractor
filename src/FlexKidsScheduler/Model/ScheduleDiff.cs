namespace FlexKidsScheduler.Model
{
    using System;
    using Repository.Model;

    public struct ScheduleDiff
    {
        public ScheduleStatus Status { get; set; }

        public DateTime Start => Schedule.StartDateTime;

        public Schedule Schedule { get; set; }
    }
}