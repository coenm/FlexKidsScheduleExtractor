namespace FlexKidsScheduler
{
    using System;

    public class DateTimeProvider : IDateTimeProvider
    {
        public static readonly IDateTimeProvider Instance = new DateTimeProvider();

        private DateTimeProvider() { }

        public DateTime Today => DateTime.Today;

        public DateTime Now => DateTime.Now;
    }
}
