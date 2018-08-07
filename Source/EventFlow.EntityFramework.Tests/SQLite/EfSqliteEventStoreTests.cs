using EventFlow.Configuration;
using EventFlow.EntityFramework.Extensions;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Suites;
using NUnit.Framework;

namespace EventFlow.EntityFramework.Tests.SQLite
{
    [Category(Categories.Integration)]
    public class EfSqliteEventStoreTests : TestSuiteForEventStore
    {
        private readonly EntityFrameworkConfiguration _configuration;

        public EfSqliteEventStoreTests()
        {
            _configuration = EntityFrameworkConfiguration.New.SetConnectionString("DataSource=:memory:");
        }

        protected override IRootResolver CreateRootResolver(IEventFlowOptions eventFlowOptions)
        {
            return eventFlowOptions
                .ConfigureEntityFramework(_configuration)
                .ConfigureForEventStoreTest<SqliteDbContextProvider>()
                .CreateResolver();
        }
    }
}
