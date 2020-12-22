namespace Repository.Model
{
    using System.Collections.Generic;

    public class Week
    {
        public int Id { get; set; }

        public int Year { get; set; }

        public int WeekNr { get; set; }

        public string Hash { get; set; }

        public IList<Schedule> Schedules { get; set; }
    }
}