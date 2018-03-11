using Microsoft.Extensions.Logging;
using System;

namespace Spike.Extensions.Logging.Sql
{
    public static class SqlLoggerExtensions
    {
        public static ILoggerFactory AddSqlServer(this ILoggerFactory factory, Func<string, LogLevel, bool> filter = null, string connectionStr = null)
        {
            factory.AddProvider(new SqlLoggerProvider(filter, connectionStr));
            return factory;
        }

        public static ILoggerFactory AddSqlServer(this ILoggerFactory factory, LogLevel minLevel, string connectionStr)
        {
            return AddSqlServer(factory, (_, logLevel) => logLevel >= minLevel, connectionStr);
        }
    }
}