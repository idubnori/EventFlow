using System;
using EventFlow.EntityFramework.Tests.Model;
using EventFlow.PostgreSql.Connections;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EventFlow.EntityFramework.Tests.PostgreSql
{
    public class PostgreSqlDbContextProvider : IDbContextProvider<TestDbContext>, IDisposable
    {
        private readonly NpgsqlConnection _connection;

        public PostgreSqlDbContextProvider(IPostgreSqlConfiguration postgreSqlConfiguration)
        {
            _connection = new NpgsqlConnection(postgreSqlConfiguration.ConnectionString);
            _connection.Open();
        }

        public TestDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(_connection)
                .Options;
            var context = new TestDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }
}
