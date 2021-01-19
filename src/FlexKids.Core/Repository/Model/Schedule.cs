namespace FlexKids.Core.Repository.Model
{
    using System;

    public class Schedule
    {
        public int Id { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public string Location { get; set; }

        public int WeekId { get; set; }

        public Week Week { get; set; }
    }
}
