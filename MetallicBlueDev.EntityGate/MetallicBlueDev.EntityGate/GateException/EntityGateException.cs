using System;
using System.Runtime.Serialization;

namespace MetallicBlueDev.EntityGate.GateException
{
    /// <summary>
    /// Error in internal in the connection with the database.
    /// </summary>
    [Serializable()]
    public class EntityGateException : Exception
    {
        /// <summary>
        /// New link error with the database.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="inner">Internal error.</param>
        public EntityGateException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// New link error with the database.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected EntityGateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// New link error with the database.
        /// </summary>
        public EntityGateException() : this(null)
        {
        }

        /// <summary>
        /// New link error with the database.
        /// </summary>
        /// <param name="message">Error message.</param>
        public EntityGateException(string message) : this(message, null)
        {
        }
    }
}

