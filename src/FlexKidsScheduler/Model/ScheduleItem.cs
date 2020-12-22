namespace FlexKidsScheduler.Model
{
    using System;

    public class ScheduleItem
    {
        public string Location { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }
    }
}