using EventFlow.Configuration;
using EventFlow.EntityFramework.Extensions;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Suites;
using NUnit.Framework;

namespace EventFlow.EntityFramework.Tests.SQLite
{
    [Category(Categories.Integration)]
    public class EfSqliteSnapshotTests : TestSuiteForSnapshotStore
    {
        private readonly EntityFrameworkConfiguration _configuration;

        public EfSqliteSnapshotTests()
        {
            _configuration = EntityFrameworkConfiguration.New.SetConnectionString("DataSource=:memory:");
        }

        protected override IRootResolver CreateRootResolver(IEventFlowOptions eventFlowOptions)
        {
            return eventFlowOptions
                .ConfigureEntityFramework(_configuration)
                .ConfigureForSnapshotStoreTest<SqliteDbContextProvider>()
                .CreateResolver();
        }
    }
}
