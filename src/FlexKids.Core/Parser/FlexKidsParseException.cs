namespace FlexKids.Core.Parser
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class FlexKidsParseException : Exception
    {
        public FlexKidsParseException()
        {
        }

        // Without this constructor, deserialization will fail
        protected FlexKidsParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}