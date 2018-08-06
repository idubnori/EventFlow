using System;
using EventFlow.EntityFramework.Tests.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.EntityFramework.Tests.SQLite
{
    public class SqliteDbContextProvider : IDbContextProvider<TestDbContext>, IDisposable
    {
        private readonly DbContextOptions<TestDbContext> _options;

        public SqliteDbContextProvider()
        {
            // In-memory database only exists while the connection is open
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            _options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(connection)
                .Options;
        }

        public TestDbContext CreateContext()
        {
            var context = new TestDbContext(_options);
            context.Database.EnsureCreated();
            return context;
        }

        public void Dispose()
        {
        }
    }
}
