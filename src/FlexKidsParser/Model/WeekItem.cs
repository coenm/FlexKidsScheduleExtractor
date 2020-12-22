namespace FlexKidsParser.Model
{
    public class WeekItem
    {
        public WeekItem(int weekNr, int year)
        {
            Year = year;
            WeekNr = weekNr;
        }

        public int Year { get; set; }
        public int WeekNr { get; set; }
    }
}