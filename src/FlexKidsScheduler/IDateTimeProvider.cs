namespace FlexKidsScheduler
{
    using System;

    public interface IDateTimeProvider
    {
        DateTime Today { get; }

        DateTime Now { get; }
    }
}