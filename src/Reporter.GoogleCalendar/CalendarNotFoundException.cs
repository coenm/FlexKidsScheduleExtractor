namespace Reporter.GoogleCalendar
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class CalendarNotFoundException : Exception
    {
        public CalendarNotFoundException(string calendarId)
        {
            CalendarId = calendarId;
        }

        // Without this constructor, deserialization will fail
        protected CalendarNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            CalendarId = info.GetString($"{nameof(CalendarId)}") ?? string.Empty;
        }

        public string CalendarId { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue($"{nameof(CalendarId)}", CalendarId);
            base.GetObjectData(info, context);
        }
    }
}