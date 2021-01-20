namespace FlexKids.Core.Scheduler.Model
{
    using FlexKids.Core.Repository.Model;

    public class WeekAndHtml
    {
        public WeekSchedule WeekSchedule { get; set; }

        public string Html { get; set; }

        public string Hash { get; set; }

        public bool ScheduleChanged { get; set; }
    }
}