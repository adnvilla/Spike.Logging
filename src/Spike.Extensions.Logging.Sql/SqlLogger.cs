using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Spike.Extensions.Logging.Sql
{
    public class SqlLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly SqlHelper _helper;
        private readonly int MessageMaxLength = 4000;

        public SqlLogger(string categoryName, Func<string, LogLevel, bool> filter, string connectionString)
        {
            _categoryName = categoryName;
            _filter = filter;
            _helper = new SqlHelper(connectionString);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }
            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (exception != null)
            {
                message += "\n" + exception;
            }

            message = message.Length > MessageMaxLength ? message.Substring(0, MessageMaxLength) : message;
            EventLog eventLog = new EventLog
            {
                Message = message,
                EventId = eventId.Id,
                LogLevel = logLevel.ToString(),
                CreatedTime = DateTime.UtcNow
            };
            _helper.InsertLog(eventLog);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (_filter == null || _filter(_categoryName, logLevel));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        private class SqlHelper
        {
            private string ConnectionString { get; }

            public SqlHelper(string connectionStr)
            {
                ConnectionString = connectionStr;
            }

            private bool ExecuteNonQuery(string commandStr, List<SqlParameter> paramList)
            {
                bool result;
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    using (SqlCommand command = new SqlCommand(commandStr, conn))
                    {
                        command.Parameters.AddRange(paramList.ToArray());
                        int count = command.ExecuteNonQuery();
                        result = count > 0;
                    }
                }
                return result;
            }

            public bool InsertLog(EventLog log)
            {
                string command = @"INSERT INTO [dbo].[EventLog] ([EventID],[LogLevel],[Message],[CreatedTime]) VALUES (@EventID, @LogLevel, @Message, @CreatedTime)";
                List<SqlParameter> paramList = new List<SqlParameter>
                {
                    new SqlParameter("EventID", log.EventId),
                    new SqlParameter("LogLevel", log.LogLevel),
                    new SqlParameter("Message", log.Message),
                    new SqlParameter("CreatedTime", log.CreatedTime)
                };
                return ExecuteNonQuery(command, paramList);
            }
        }

        public class EventLog
        {
            public int Id { get; set; }
            public int? EventId { get; set; }
            public string LogLevel { get; set; }
            public string Message { get; set; }
            public DateTime? CreatedTime { get; set; }
        }
    }
}