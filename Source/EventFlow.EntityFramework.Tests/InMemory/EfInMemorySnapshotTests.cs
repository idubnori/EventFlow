using EventFlow.Configuration;
using EventFlow.EntityFramework.Extensions;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Suites;
using NUnit.Framework;

namespace EventFlow.EntityFramework.Tests.InMemory
{
    [Category(Categories.Integration)]
    [Ignore("Ignore to not support linq2db.EntityFrameworkCore, as temporal.")]
    public class EfInMemorySnapshotTests : TestSuiteForSnapshotStore
    {
        protected override IRootResolver CreateRootResolver(IEventFlowOptions eventFlowOptions)
        {
            return eventFlowOptions
                .ConfigureEntityFramework()
                .ConfigureForSnapshotStoreTest<InMemoryDbContextProvider>()
                .CreateResolver();
        }
    }
}
