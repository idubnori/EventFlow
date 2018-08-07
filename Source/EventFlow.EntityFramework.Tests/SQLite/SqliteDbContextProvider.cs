using System;
using EventFlow.EntityFramework.Tests.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.EntityFramework.Tests.SQLite
{
    public class SqliteDbContextProvider : IDbContextProvider<TestDbContext>, IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<TestDbContext> _options;
        
        public SqliteDbContextProvider(IEntityFrameworkConfiguration configuration)
        {
            // In-memory database only exists while the connection is open
            _connection = new SqliteConnection(configuration.ConnectionString);
            _connection.Open();

            _options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(_connection)
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
            _connection.Dispose();
        }
    }
}
