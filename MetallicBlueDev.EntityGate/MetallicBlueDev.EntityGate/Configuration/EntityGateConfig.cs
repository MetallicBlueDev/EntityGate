namespace MetallicBlueDev.EntityGate.Configuration
{
    /// <summary>
    /// Represents a configuration portion.
    /// </summary>
    internal class EntityGateConfig
    {
        /// <summary>
        /// Name of the connection.
        /// <see cref="System.Configuration.ConnectionStringsSection"/>
        /// </summary>
        internal string ConnectionName { get; set; }

        /// <summary>
        /// Maximum number of connection attempts.
        /// </summary>
        internal int MaximumNumberOfAttempts { get; set; } = 5;

        /// <summary>
        /// Waiting time between each attempt
        /// </summary>
        internal int AttemptDelay { get; set; } = 1000;

        /// <summary>
        /// Maximum wait time before a connection failure.
        /// </summary>
        internal int Timeout { get; set; } = 30;

        /// <summary>
        /// Default value of LazyLoading.
        /// </summary>
        internal bool LazyLoading { get; set; } = true;

        /// <summary>
        /// Determines if the backup of the original values is performed automatically.
        /// </summary>
        internal bool AutomaticCheckOfOriginValues { get; set; } = true;
    }
}
