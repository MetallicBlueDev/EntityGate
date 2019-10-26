using System;
using System.Runtime.Serialization;

namespace MetallicBlueDev.EntityGate.GateException
{
    /// <summary>
    /// Error in the search for information by reflection.
    /// </summary>
    [Serializable()]
    public class ReflectionEntityGateException : EntityGateException
    {
        /// <summary>
        /// New error in the search for information by reflection.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="inner">Internal error.</param>
        public ReflectionEntityGateException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// New error in the search for information by reflection.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected ReflectionEntityGateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// New error in the search for information by reflection.
        /// </summary>
        public ReflectionEntityGateException() : this(null)
        {
        }

        /// <summary>
        /// New error in the search for information by reflection.
        /// </summary>
        /// <param name="message">Error message.</param>
        public ReflectionEntityGateException(string message) : this(message, null)
        {
        }
    }
}

