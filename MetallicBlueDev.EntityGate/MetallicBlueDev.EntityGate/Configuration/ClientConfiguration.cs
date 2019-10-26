using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using MetallicBlueDev.EntityGate.Extensions;
using MetallicBlueDev.EntityGate.Properties;

namespace MetallicBlueDev.EntityGate.Configuration
{
    /// <summary>
    /// Client configuration.
    /// </summary>
    [Serializable()]
    public sealed class ClientConfiguration
    {
        private readonly IEntityGateObject gate;

        private int maximumNumberOfAttempts = 0;
        private int attemptDelay = 0;
        private string connectionString = null;
        private int timeout = 0;

        /// <summary>
        /// Default value of LazyLoading.
        /// </summary>
        public bool LazyLoading { get; internal set; } = true;

        /// <summary>
        /// Determines whether the configuration has been updated.
        /// </summary>
        public bool Updated { get; private set; } = false;

        /// <summary>
        /// Maximum number of attempts.
        /// </summary>
        public int MaximumNumberOfAttempts
        {
            get => maximumNumberOfAttempts;
            set
            {
                if (value > 0)
                {
                    maximumNumberOfAttempts = value;
                    ConfigurationUpdated();
                }
            }
        }

        /// <summary>
        /// Waiting time (in milliseconds) before the next attempt.
        /// </summary>
        public int AttemptDelay
        {
            get => attemptDelay;
            set
            {
                if (value > 0)
                {
                    attemptDelay = value;
                    ConfigurationUpdated();
                }
            }
        }

        /// <summary>
        /// The raw connection string.
        /// </summary>
        public string ConnectionString
        {
            get => connectionString;
            set
            {
                if (value.IsNotNullOrEmpty())
                {
                    connectionString = value;
                    ConfigurationUpdated();
                }
            }
        }

        /// <summary>
        /// Maximum time (in seconds) to run a query.
        /// </summary>
        public int Timeout
        {
            get => timeout;
            set
            {
                if (value > 3)
                {
                    timeout = value;
                    ConfigurationUpdated();
                }
            }
        }

        /// <summary>
        /// Get or set the status of the notification.
        /// </summary>
        public bool CanUseNotification { get; set; }

        /// <summary>
        /// Determines if the backup of the original values is performed automatically.
        ///
        /// If the main entity implements the <see cref="InterfacedObject.IEntityObjectArchival"/> interface, the backup is automatically enabled.
        /// </summary>
        public bool AutomaticCheckOfOriginalValues { get; set; }

        /// <summary>
        /// Log internal messages. Include messages from Entity Framework.
        /// </summary>
        public Action<string> Log { get; set; } = null;

        /// <summary>
        /// Determines whether it is possible to use the event log.
        /// The <see cref="Log"/> method must be defined.
        /// </summary>
        public bool CanUseLogging => Log != null;

        /// <summary>
        /// New configuration.
        /// </summary>
        /// <param name="gate">EntityGateObject Instance</param>
        internal ClientConfiguration(IEntityGateObject gate)
        {
            this.gate = gate;

            var defaultConfig = EntityGateConfigLoader.GetFirstConfig();

            if (defaultConfig != null)
            {
                ChangeConnectionString(defaultConfig.ConnectionName);
            }
        }

        /// <summary>
        /// Change the connection string.
        /// </summary>
        /// <param name="connectionName">Name of the connection.</param>
        public void ChangeConnectionString(string connectionName)
        {
            var currentConfig = EntityGateConfigLoader.GetConfig(connectionName);

            if (currentConfig != null)
            {
                CopyConfiguration(currentConfig);

                if (CanUseLogging)
                {
                    Log(string.Format(CultureInfo.InvariantCulture, Resources.ConfigurationHasBeenLoaded, connectionName));
                }
            }
            else if (CanUseLogging)
            {
                Log(string.Format(CultureInfo.InvariantCulture, Resources.UnableToFindConnectionString, connectionName));
            }
        }

        /// <summary>
        /// Update the connection configuration.
        /// </summary>
        /// <param name="connection">Instance of the connection.</param>
        internal void Update(IDbConnection connection)
        {
            var builder = CreateBuilder();
            connection.ConnectionString = builder.ConnectionString;
        }

        /// <summary>
        /// Creating a connection string.
        /// </summary>
        /// <returns></returns>
        internal SqlConnectionStringBuilder CreateBuilder()
        {
            var sqlBuilder = new SqlConnectionStringBuilder
            {
                IntegratedSecurity = false,
                PersistSecurityInfo = true,
                MultipleActiveResultSets = false,
                ConnectTimeout = Timeout,
                ConnectionString = ConnectionString
            };

            ConfigurationUpToDate();

            return sqlBuilder;
        }

        /// <summary>
        /// Signal that the configuration has changed.
        /// </summary>
        internal void ConfigurationUpdated()
        {
            if (!Updated)
            {
                Updated = true;

                if (CanUseLogging)
                {
                    Log(Resources.ConfigurationIsUpToDate);
                }
            }
        }

        /// <summary>
        /// Copy of the configuration.
        /// </summary>
        /// <param name="config"></param>
        private void CopyConfiguration(EntityGateConfig config)
        {
            ConnectionString = EntityGateConfigLoader.GetConnectionString(config.ConnectionName);

            MaximumNumberOfAttempts = config.MaximumNumberOfAttempts;
            AttemptDelay = config.AttemptDelay;
            LazyLoading = config.LazyLoading;
            Timeout = config.Timeout;
            AutomaticCheckOfOriginalValues = config.AutomaticCheckOfOriginalValues;
        }

        /// <summary>
        /// Indicates that the context is synchronized to this configuration.
        /// </summary>
        private void ConfigurationUpToDate()
        {
            if (Updated)
            {
                Updated = false;

                if (CanUseLogging)
                {
                    Log(Resources.ConfigurationMustBeUpdated);
                }
            }
        }
    }
}

