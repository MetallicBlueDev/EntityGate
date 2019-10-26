using System;
using System.Runtime.Serialization;

namespace MetallicBlueDev.EntityGate.GateException
{
    /// <summary>
    /// An error occurred in the configuration.
    /// </summary>
    [Serializable()]
    public class ConfigurationEntityGateException : EntityGateException
    {
        /// <summary>
        /// New configuration error.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="inner">Internal error.</param>
        public ConfigurationEntityGateException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// New configuration error.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected ConfigurationEntityGateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// New configuration error.
        /// </summary>
        public ConfigurationEntityGateException() : this(null)
        {
        }

        /// <summary>
        /// New configuration error.
        /// </summary>
        /// <param name="message">Error message.</param>
        public ConfigurationEntityGateException(string message) : this(message, null)
        {
        }
    }
}

