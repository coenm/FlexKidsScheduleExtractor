namespace FlexKids.Core.Scheduler
{
    using System;

    public interface IDateTimeProvider
    {
        DateTime Today { get; }

        DateTime Now { get; }
    }
}