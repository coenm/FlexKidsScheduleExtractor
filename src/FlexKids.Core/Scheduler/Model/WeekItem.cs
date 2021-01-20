namespace FlexKids.Core.Scheduler.Model
{
    public readonly struct WeekItem
    {
        public WeekItem(int weekNr, int year)
        {
            Year = year;
            WeekNr = weekNr;
        }

        public int Year { get; }

        public int WeekNr { get; }
    }
}