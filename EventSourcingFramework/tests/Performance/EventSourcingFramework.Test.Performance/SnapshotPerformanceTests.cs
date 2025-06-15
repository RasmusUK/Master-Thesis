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
public class SnapshotPerformanceTests : MongoIntegrationTestBase
{
    private readonly ITestOutputHelper testOutputHelper;

    public SnapshotPerformanceTests(
            IMongoDbService mongoDbService,
            IReplayContext replayContext, ITestOutputHelper testOutputHelper)
        : base(mongoDbService, replayContext)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Trait("Category", "Performance")]
    [Theory]
    [InlineData(10000 )]
    public async Task ReplaySnapshotPerformance(int eventCountSnapshot)
    {
        var eventThresholds = new[] { 10000, 25000, 50000, 100000, 250000, 500000, 1000000 };

        var eventCount = 1000000;
        
        var provider = ServiceProvider.BuildServiceProviderWithSettings(
            new Dictionary<string, string>
            {
                ["EventSourcing:Snapshot:Trigger:EventThreshold"] = $"{eventCountSnapshot}",
                ["EventSourcing:Snapshot:Retention:MaxCount"] = "100000",
                ["EventSourcing:EnablePersonalDataStore"] = "false",
                ["EventSourcing:Snapshot:Enabled"] = "true"
            }
        );
        
        var repo = provider.GetRequiredService<IRepository<TestEntity>>();
        
        var durations = new List<long>();
        var entity = TestEntityFactory.CreateEntity();
        
        await repo.CreateAsync(entity);
        var sw = Stopwatch.StartNew();
        
        for (var i = 1; i <= eventCount; i++)
        {
            if (i % 10000 == 0 || i % 25000 == 0)
            {
                testOutputHelper.WriteLine($"Creating entity {i} of {eventCount} with max duration {durations.OrderByDescending(x => x).Take(1).First()} ms" );
            }
            
            entity.Id = Guid.NewGuid();
            
            sw = Stopwatch.StartNew();
            await repo.CreateAsync(entity);
            sw.Stop();
            
            durations.Add(sw.ElapsedMilliseconds);
        }
        
        var replayService = provider.GetRequiredService<IReplayService>();
        
        for (var i = 0; i < eventThresholds.Length; i++)
        {
            sw = Stopwatch.StartNew();
            await replayService.ReplayUntilEventNumberAsync(eventThresholds[i], useSnapshot: false);
            sw.Stop();
                
            testOutputHelper.WriteLine($"Replay without snapshot for {eventThresholds[i]} events took: {sw.ElapsedMilliseconds} ms");
        }
        
        for (var i = 0; i < eventThresholds.Length; i++)
        {
            sw = Stopwatch.StartNew();
            await replayService.ReplayUntilEventNumberAsync(eventThresholds[i], useSnapshot: true);
            sw.Stop();
            
            testOutputHelper.WriteLine($"Replay with snapshot for {eventThresholds[i]} events took: {sw.ElapsedMilliseconds} ms");
        }
    }
}