using Microsoft.Extensions.Logging;
using NPoco;
using Respawn;
using Shouldly;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Xunit;

namespace Spike.Extensions.Logging.Sql.Tests
{
    public class LoggingTests : IAsyncLifetime
    {
        private const string Name = "SqlServerTest";
        private const string TestMessage = "This is a test";
        private SqlConnection _connection;
        private Database _database;

        public async Task InitializeAsync()
        {
            var isAppVeyor = Environment.GetEnvironmentVariable("Appveyor")?.ToUpperInvariant() == "TRUE";
            var connString =
                isAppVeyor
                    ? @"Server=(local)\SQL2017;Database=tempdb;User ID=sa;Password=Password12!"
                    : @"Server=(LocalDb)\MSSQLLocalDB;Database=tempdb;Integrated Security=True";

            using (var connection = new SqlConnection(connString))
            {
                await connection.OpenAsync();
                using (var database = new Database(connection))
                {
                    await database.ExecuteAsync(@"IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'SqlServerTests') alter database SqlServerTests set single_user with rollback immediate");
                    await database.ExecuteAsync(@"DROP DATABASE IF EXISTS SqlServerTests");
                    await database.ExecuteAsync("create database [SqlServerTests]");
                }
            }

            connString =
                isAppVeyor
                    ? @"Server=(local)\SQL2017;Database=SqlServerTests;User ID=sa;Password=Password12!"
                    : @"Server=(LocalDb)\MSSQLLocalDB;Database=SqlServerTests;Integrated Security=True";

            _connection = new SqlConnection(connString);
            _connection.Open();

            _database = new Database(_connection);

            await _database.ExecuteAsync("create table EventLog ([EventID][int],[LogLevel][text],[Message][text],[CreatedTime][datetime])");
        }

        public Task DisposeAsync()
        {
            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
            return Task.FromResult(0);
        }

        private SqlLogger SetUp()
        {
            var provider = new SqlLoggerProvider((_, logLevel) => logLevel >= LogLevel.Information, _connection.ConnectionString);
            var logger = (SqlLogger)provider.CreateLogger(Name);

            var checkPoint = new Checkpoint();

            checkPoint.Reset(_connection.ConnectionString);

            return logger;
        }

        [Fact]
        public async Task ShouldBe()
        {
            var logger = SetUp();

            logger.Log(LogLevel.Information, 1, TestMessage, null, (s, exception) => TestMessage);

            var result = await _database.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM EventLog");

            result.ShouldBe(1);
        }
    }
}