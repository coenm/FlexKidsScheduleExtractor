namespace FlexKids.Core.Repository.Model
{
    using System.Collections.Generic;

    public class WeekSchedule
    {
        public WeekSchedule()
        {
            Shifts = new List<SingleShift>();
        }

        public int Id { get; set; }

        public int Year { get; set; }

        public int WeekNumber { get; set; }

        public List<SingleShift> Shifts { get; set; }
    }
}