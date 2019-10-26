using System;
using System.Runtime.Serialization;

namespace MetallicBlueDev.EntityGate.GateException
{
    /// <summary>
    /// Internal error to the provider.
    /// </summary>
    [Serializable()]
    public class EntityGateProviderException : EntityGateException
    {
        /// <summary>
        /// New context error.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="inner">Internal error.</param>
        public EntityGateProviderException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// New context error.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected EntityGateProviderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// New context error.
        /// </summary>
        public EntityGateProviderException() : this(null)
        {
        }

        /// <summary>
        /// New context error.
        /// </summary>
        /// <param name="message">Error message.</param>
        public EntityGateProviderException(string message) : this(message, null)
        {
        }
    }
}

