using Serilog.Configuration;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using static ASql.ASqlManager;

namespace Serilog.Sinks.Database
{
    public static class LoggerConfigurationDatabaseExtensions
    {
        /// <summary>
        ///     Adds a sink that writes log events to a MySQL database.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The connection string to MySQL database.</param>
        /// <param name="tableName">The name of the MySQL table to store log.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="storeTimestampInUtc">Store timestamp in UTC format</param>
        /// <param name="batchSize">Number of log messages to be sent as batch. Supported range is between 1 and 1000</param>
        /// <param name="levelSwitch">
        /// A switch allowing the pass-through minimum level to be changed at runtime.
        /// </param>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration Database(
            this LoggerSinkConfiguration loggerConfiguration,
            DBType databaseType,
            string connectionString,
            string tableName = "Logs",
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            bool storeTimestampInUtc = false,
            uint batchSize = 100,
            LoggingLevelSwitch levelSwitch = null)
        {
            if (loggerConfiguration == null)
                throw new ArgumentNullException(nameof(loggerConfiguration));

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            if (batchSize < 1 || batchSize > 1000)
                throw new ArgumentOutOfRangeException("[batchSize] argument must be between 1 and 1000 inclusive");

            try
            {
                return loggerConfiguration.Sink(
                    new DatabaseSink(databaseType,connectionString, tableName, storeTimestampInUtc),
                    restrictedToMinimumLevel,
                    levelSwitch);
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine(ex.Message);

                throw;
            }
        }
    }
}
