using System.Diagnostics;
using EventSourcingFramework.Application.Interfaces;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Persistence.Interfaces;
using EventSourcingFramework.Test.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingFramework.Test.Performance;

[Collection("Integration")]
public class RepositoryPerformanceTests : MongoIntegrationTestBase
{
    public RepositoryPerformanceTests(
        IMongoDbService mongoDbService,
        IGlobalReplayContext replayContext
    )
        : base(mongoDbService, replayContext) { }

    [Trait("Category", "Performance")]
    [Theory]
    [InlineData("S1", 100, true, true, true)]
    [InlineData("S1", 100, true, true, false)]
    [InlineData("S1", 100, true, false, true)]
    [InlineData("S1", 100, true, false, false)]
    [InlineData("S1", 100, false, true, false)]
    [InlineData("S2", 100, true, true, true)]
    [InlineData("S2", 100, true, true, false)]
    [InlineData("S2", 100, true, false, true)]
    [InlineData("S2", 100, true, false, false)]
    [InlineData("S2", 100, false, true, false)]
    [InlineData("S3", 100, true, true, true)]
    [InlineData("S3", 100, true, true, false)]
    [InlineData("S3", 100, true, false, true)]
    [InlineData("S3", 100, true, false, false)]
    [InlineData("S3", 100, false, true, false)]
    [InlineData("S4", 100, true, true, true)]
    [InlineData("S4", 100, true, true, false)]
    [InlineData("S4", 100, true, false, true)]
    [InlineData("S4", 100, true, false, false)]
    [InlineData("S4", 100, false, true, false)]
    [InlineData("S5", 100, true, true, true)]
    [InlineData("S5", 100, true, true, false)]
    [InlineData("S5", 100, true, false, true)]
    [InlineData("S5", 100, true, false, false)]
    [InlineData("S5", 100, false, true, false)]
    public async Task CreatePerformance(
        string entitySize,
        int count,
        bool eventStore,
        bool entityStore,
        bool personalStore
    )
    {
        var provider = ServiceProvider.BuildServiceProviderWithSettings(
            new Dictionary<string, string>
            {
                ["EventSourcing:EnableEventStore"] = $"{eventStore}",
                ["EventSourcing:EnableEntityStore"] = $"{entityStore}",
                ["EventSourcing:EnablePersonalDataStore"] = $"{personalStore}",
            }
        );

        var repo = provider.GetRequiredService<IRepository<TestEntity>>();
        var durations = new List<long>();
        var entity = TestEntityFactory.CreateEntityBySize(entitySize);

        //Warmup
        await repo.CreateAsync(entity);

        for (var i = 0; i < count; i++)
        {
            entity.Id = Guid.NewGuid();
            var sw = Stopwatch.StartNew();
            await repo.CreateAsync(entity);
            sw.Stop();
            durations.Add(sw.ElapsedMilliseconds);
        }

        CsvLogger.LogRepo(
            nameof(CreatePerformance),
            eventStore,
            entityStore,
            personalStore,
            entitySize,
            TestEntityFactory.GetBsonSizeInMb(entity),
            count,
            TestEntityFactory.GetPropertyCountBySizeName(entitySize),
            TestEntityFactory.GetNodeCountBySizeName(entitySize),
            durations
        );
    }
}
