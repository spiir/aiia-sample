using System;
using System.Runtime.Serialization;

namespace Aiia.Sample.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException() { }

        public UserNotFoundException(string message)
            : base(message) { }

        public UserNotFoundException(string message, Exception inner)
            : base(message, inner) { }

        protected UserNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
