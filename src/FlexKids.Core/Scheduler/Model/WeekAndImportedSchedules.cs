namespace FlexKids.Core.Scheduler.Model
{
    using System.Collections.Generic;
    using FlexKids.Core.Repository.Model;

    public class WeekAndImportedSchedules
    {
        public WeekSchedule WeekSchedule { get; set; }

        public List<ScheduleItem> ScheduleItems { get; set; }
    }
}