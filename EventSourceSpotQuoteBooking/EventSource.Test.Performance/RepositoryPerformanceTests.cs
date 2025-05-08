using System.Diagnostics;
using EventSource.Application.Interfaces;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Interfaces;
using EventSource.Test.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace EventSource.Test.Performance;

[Collection("Integration")]
public class RepositoryPerformanceTests : MongoIntegrationTestBase
{
    private static readonly string createPerformancePath = Path.Combine(
        CsvLogger.TestResultsDir,
        $"CreatePerformance.{DateTime.UtcNow:yyyyMMddHHmmss}.csv"
    );

    public RepositoryPerformanceTests(
        IMongoDbService mongoDbService,
        IGlobalReplayContext replayContext
    )
        : base(mongoDbService, replayContext) { }

    [Theory]
    [InlineData("Small", 1000, true, true, true)]
    [InlineData("Small", 1000, true, true, false)]
    [InlineData("Small", 1000, true, false, true)]
    [InlineData("Small", 1000, true, false, false)]
    [InlineData("Small", 1000, false, true, false)]
    [InlineData("Small", 10000, true, true, true)]
    [InlineData("Small", 10000, true, true, false)]
    [InlineData("Small", 10000, true, false, true)]
    [InlineData("Small", 10000, true, false, false)]
    [InlineData("Small", 10000, false, true, false)]
    [InlineData("Small", 100000, true, true, true)]
    [InlineData("Small", 100000, true, true, false)]
    [InlineData("Small", 100000, true, false, true)]
    [InlineData("Small", 100000, true, false, false)]
    [InlineData("Small", 100000, false, true, false)]
    [InlineData("Medium", 1000, true, true, true)]
    [InlineData("Medium", 1000, true, true, false)]
    [InlineData("Medium", 1000, true, false, true)]
    [InlineData("Medium", 1000, true, false, false)]
    [InlineData("Medium", 1000, false, true, false)]
    [InlineData("Medium", 10000, true, true, true)]
    [InlineData("Medium", 10000, true, true, false)]
    [InlineData("Medium", 10000, true, false, true)]
    [InlineData("Medium", 10000, true, false, false)]
    [InlineData("Medium", 10000, false, true, false)]
    [InlineData("Medium", 100000, true, true, true)]
    [InlineData("Medium", 100000, true, true, false)]
    [InlineData("Medium", 100000, true, false, true)]
    [InlineData("Medium", 100000, true, false, false)]
    [InlineData("Medium", 100000, false, true, false)]
    [InlineData("Large", 1000, true, true, true)]
    [InlineData("Large", 1000, true, true, false)]
    [InlineData("Large", 1000, true, false, true)]
    [InlineData("Large", 1000, true, false, false)]
    [InlineData("Large", 1000, false, true, false)]
    [InlineData("Large", 10000, true, true, true)]
    [InlineData("Large", 10000, true, true, false)]
    [InlineData("Large", 10000, true, false, true)]
    [InlineData("Large", 10000, true, false, false)]
    [InlineData("Large", 10000, false, true, false)]
    [InlineData("Large", 100000, true, true, true)]
    [InlineData("Large", 100000, true, true, false)]
    [InlineData("Large", 100000, true, false, true)]
    [InlineData("Large", 100000, true, false, false)]
    [InlineData("Large", 100000, false, true, false)]
    public async Task CreatePerformance(
        string entitySize,
        int count,
        bool eventStore,
        bool entityStore,
        bool personalStore
    )
    {
        for (var j = 0; j < 1; j++)
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

            var insertDurations = new List<long>();
            var entity = TestEntityFactory.CreateEntityBySize(entitySize);
            await repo.CreateAsync(entity);

            for (var i = 0; i < count; i++)
            {
                var insertWatch = Stopwatch.StartNew();
                entity.Id = Guid.NewGuid();
                await repo.CreateAsync(entity);
                insertWatch.Stop();
                insertDurations.Add(insertWatch.ElapsedMilliseconds);
            }

            CsvLogger.LogRepoCreate(
                createPerformancePath,
                nameof(CreatePerformance),
                eventStore,
                entityStore,
                personalStore,
                entitySize,
                TestEntityFactory.GetSizeInMb(entity),
                count,
                TestEntityFactory.GetPropertyCountBySizeName(entitySize),
                TestEntityFactory.GetNodeCountBySizeName(entitySize),
                insertDurations
            );
        }
    }
}
