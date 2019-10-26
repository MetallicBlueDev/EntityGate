using System;
using System.Runtime.Serialization;

namespace MetallicBlueDev.EntityGate.GateException
{
    /// <summary>
    /// Transaction cancellation (may be automatically triggered internally or externally).
    /// </summary>
    [Serializable()]
    public class TransactionCanceledEntityGateException : EntityGateException
    {
        /// <summary>
        /// New transaction cancellation.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="inner">Internal error.</param>
        public TransactionCanceledEntityGateException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// New transaction cancellation.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected TransactionCanceledEntityGateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// New transaction cancellation.
        /// </summary>
        public TransactionCanceledEntityGateException() : this(null)
        {
        }

        /// <summary>
        /// New transaction cancellation.
        /// </summary>
        /// <param name="message">Error message.</param>
        public TransactionCanceledEntityGateException(string message) : this(message, null)
        {
        }
    }
}

