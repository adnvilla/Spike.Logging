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


#if !NETCORE1_0  && !NET45

        public static ILoggingBuilder AddSqlServer(this ILoggingBuilder builder, Func<string, LogLevel, bool> filter = null, string connectionStr = null)
        {
            using (var provider = new SqlLoggerProvider(filter, connectionStr))
            {
                builder.AddProvider(provider);
            }

            return builder;
        }

        public static ILoggingBuilder AddSqlServer(this ILoggingBuilder builder, LogLevel minLevel, string connectionStr)
        {
            using (var provider = new SqlLoggerProvider((_, logLevel) => logLevel >= minLevel, connectionStr))
            {
                builder.AddProvider(provider);
            }

            return builder;
        }

#endif
    }
}