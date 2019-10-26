using System;
using System.Data.SqlClient;
using System.Globalization;
using MetallicBlueDev.EntityGate.Helpers;
using MetallicBlueDev.EntityGate.Properties;

namespace MetallicBlueDev.EntityGate.Extensions
{
    internal static class EntityGateObjectExtensions
    {
        /// <summary>
        /// Marks the errors of the SQL exception.
        /// </summary>
        /// <param name="gate">The instance of EntityGate.</param>
        /// <param name="ex">The internal error.</param>
        internal static void LogException(this IEntityGateObject gate, Exception ex)
        {
            if (gate.Configuration.CanUseLogging)
            {
                var lastQuery = gate.Token.SqlStatement.IsNotNullOrEmpty() ? gate.Token.SqlStatement : Resources.UnknownQuery;

                gate.Configuration.Log(string.Format(CultureInfo.InvariantCulture, Resources.UnableToExecuteRequest, lastQuery));

                var sqlEx = ReflectionHelper.GetSqlException(ex);

                if (sqlEx != null)
                {
                    foreach (SqlError currentError in sqlEx.Errors)
                    {
                        gate.Configuration.Log(currentError.ToString());
                    }
                }
            }
        }
    }
}
