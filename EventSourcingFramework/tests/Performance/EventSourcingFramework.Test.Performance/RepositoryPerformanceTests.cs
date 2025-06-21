using System.Diagnostics;
using EventSourcingFramework.Application.Abstractions.Replay;
using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Test.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace EventSourcingFramework.Test.Performance;

[Collection("Integration")]
public class RepositoryPerformanceTests : MongoIntegrationTestBase
{
    private readonly ITestOutputHelper testOutputHelper;

    public RepositoryPerformanceTests(
        IMongoDbService mongoDbService,
        IReplayContext replayContext, ITestOutputHelper testOutputHelper)
        : base(mongoDbService, replayContext)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Trait("Category", "Performance")]
    [Theory]
    [InlineData(10000, true, true, true)]
    [InlineData(10000, true, true, false)]
    [InlineData(10000, true, false, false)]
    public async Task CreatePerformance(
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
                ["EventSourcing:EnablePersonalDataStore"] = $"{personalStore}"
            }
        );

        var repo = provider.GetRequiredService<IRepository<TestEntity>>();
        var durations = new List<long>();
        var entity = TestEntityFactory.CreateEntity();

        await repo.CreateAsync(entity);

        for (var i = 0; i < count; i++)
        {
            entity.Id = Guid.NewGuid();
            var sw = Stopwatch.StartNew();
            await repo.CreateAsync(entity);
            sw.Stop();
            durations.Add(sw.ElapsedMilliseconds);
        }

        testOutputHelper.WriteLine($"Average duration: {durations.Average()} ms");
    }

    [Trait("Category", "Performance")]
    [Fact]
    public async Task ReadPerformance()
    {
        var provider = ServiceProvider.BuildServiceProviderWithSettings();
        var repo = provider.GetRequiredService<IRepository<TestEntity2>>();
        var eventStore = provider.GetRequiredService<IEventStore>();
        var replayService = provider.GetRequiredService<IReplayService>();
        
        var entity = TestEntityFactory.CreateTestEntity2();
        await repo.CreateAsync(entity);
     
        for (var j = 0; j < 9; j++)
        {
            await repo.UpdateAsync(entity);
        }
     
        var sw = Stopwatch.StartNew();
        _ = await repo.ReadByIdAsync(entity.Id);
        sw.Stop();
        testOutputHelper.WriteLine($"Read full entity took: {sw.ElapsedMilliseconds} ms");
        
        sw = Stopwatch.StartNew();
        _ = await repo.ReadProjectionByFilterAsync(
            e => e.Id == entity.Id,
            e => e.Name
        );
        sw.Stop();
        testOutputHelper.WriteLine(
            $"Read projection by filter took: {sw.ElapsedMilliseconds} ms"
        );
     
        sw = Stopwatch.StartNew();
        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);
        await replayService.ReplayEventsAsync(events);
        _ = await repo.ReadByIdAsync(entity.Id);
        sw.Stop();
        testOutputHelper.WriteLine(
            $"Read entity by events took: {sw.ElapsedMilliseconds} ms"
        );
        
     
        sw = Stopwatch.StartNew();
        events = await eventStore.GetEventsByEntityIdAsync(entity.Id);
        await replayService.ReplayEventsAsync(events);
        _ = await repo.ReadProjectionByFilterAsync(
            e => e.Id == entity.Id,
            e => e.Name
        );
        sw.Stop();
        testOutputHelper.WriteLine(
            $"Read projection by events took: {sw.ElapsedMilliseconds} ms"
        );
    }
}