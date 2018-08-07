using System;
using EventFlow.Configuration;
using EventFlow.EntityFramework.Extensions;
using EventFlow.EntityFramework.Tests.Model;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Suites;
using NUnit.Framework;

namespace EventFlow.EntityFramework.Tests.SQLite
{
    [Category(Categories.Integration)]
    public class EfSqliteReadStoreTests : TestSuiteForReadModelStore
    {
        private readonly EntityFrameworkConfiguration _configuration;
        
        protected override Type ReadModelType => typeof(ThingyReadModelEntity);

        public EfSqliteReadStoreTests()
        {
            _configuration = EntityFrameworkConfiguration.New.SetConnectionString("DataSource=:memory:");
        }

        protected override IRootResolver CreateRootResolver(IEventFlowOptions eventFlowOptions)
        {
            return eventFlowOptions
                .ConfigureEntityFramework(_configuration)
                .ConfigureForReadStoreTest<SqliteDbContextProvider>()
                .CreateResolver();
        }
    }
}
