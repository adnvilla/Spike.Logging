using Microsoft.Extensions.Logging;
using System;

namespace Spike.Extensions.Logging.Sql
{
    public class SqlLoggerProvider : ILoggerProvider
    {
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly string _connectionString;

        public SqlLoggerProvider(Func<string, LogLevel, bool> filter, string connectionStr)
        {
            _filter = filter;
            _connectionString = connectionStr;
        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new SqlLogger(categoryName, _filter, _connectionString);
        }
    }
}