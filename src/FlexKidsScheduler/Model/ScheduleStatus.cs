namespace FlexKidsScheduler.Model
{
    public enum ScheduleStatus
    {
        /// <summary>
        /// Indicates that schedule is unchanged.
        /// </summary>
        Unchanged = 0,

        /// <summary>
        /// Indicates that schedule has been removed.
        /// </summary>
        Removed = 1,

        /// <summary>
        /// Indicates that the schedule is added.
        /// </summary>
        Added = 2,
    }
}