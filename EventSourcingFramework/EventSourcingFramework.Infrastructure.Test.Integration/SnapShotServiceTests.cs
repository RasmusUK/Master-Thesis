using EventSourcing.Framework.Infrastructure.Shared.Configuration.Options;
using EventSourcingFramework.Application.Abstractions;
using EventSourcingFramework.Application.Abstractions.Snapshots;
using EventSourcingFramework.Application.UseCases.ReplayContext;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Abstractions.EventStore;
using EventSourcingFramework.Infrastructure.Abstractions.MongoDb;
using EventSourcingFramework.Infrastructure.Snapshots;
using EventSourcingFramework.Test.Utilities;
using MongoDB.Driver;

namespace EventSourcingFramework.Infrastructure.Test.Integration;

[Collection("Integration")]
public class SnapshotServiceTests : MongoIntegrationTestBase
{
    private readonly ISnapshotService snapshotService;
    private readonly IEntityCollectionNameProvider collectionNameProvider;
    private readonly IRepository<TestEntity> repository;
    private readonly IEventSequenceGenerator sequenceGenerator;

    public SnapshotServiceTests(
        IMongoDbService mongoDbService,
        IGlobalReplayContext replayContext,
        ISnapshotService snapshotService,
        IEntityCollectionNameProvider collectionNameProvider,
        IRepository<TestEntity> repository,
        IEventSequenceGenerator sequenceGenerator
    )
        : base(mongoDbService, replayContext)
    {
        this.snapshotService = snapshotService;
        this.collectionNameProvider = collectionNameProvider;
        this.repository = repository;
        this.sequenceGenerator = sequenceGenerator;
    }

    [Fact]
    public async Task TakeSnapshotAsync_CreatesSnapshot_WhenCollectionHasData()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "SnapshotEntity" };
        await repository.CreateAsync(entity);
        var initialSnapshots = await snapshotService.GetAllSnapshotsAsync();

        // Act
        var snapshotId = await snapshotService.TakeSnapshotAsync();
        var allSnapshots = await snapshotService.GetAllSnapshotsAsync();

        // Assert
        Assert.NotNull(snapshotId);
        Assert.Contains(allSnapshots, s => s.SnapshotId == snapshotId);
        Assert.True(allSnapshots.Count > initialSnapshots.Count);
    }

    [Fact]
    public async Task TakeSnapshotAsync_DoesNotCreateSnapshot_WhenCollectionEmpty()
    {
        // Arrange
        var collection = MongoDbService.GetEntityCollection<TestEntity>(
            collectionNameProvider.GetCollectionName(typeof(TestEntity))
        );
        await collection.DeleteManyAsync(FilterDefinition<TestEntity>.Empty);

        var initialSnapshots = await snapshotService.GetAllSnapshotsAsync();

        // Act
        var snapshotId = await snapshotService.TakeSnapshotAsync();
        var allSnapshots = await snapshotService.GetAllSnapshotsAsync();

        // Assert
        Assert.DoesNotContain(allSnapshots, s => s.SnapshotId == snapshotId);
        Assert.Equal(initialSnapshots.Count, allSnapshots.Count);
    }

    [Fact]
    public async Task RestoreSnapshotAsync_RestoresState()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Original" };
        await repository.CreateAsync(entity);
        var snapshotId = await snapshotService.TakeSnapshotAsync();

        await repository.DeleteAsync(entity);

        // Act
        await snapshotService.RestoreSnapshotAsync(snapshotId);
        var restored = await repository.ReadByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal("Original", restored!.Name);
    }

    [Fact]
    public async Task GetLastSnapshotIdAsync_ReturnsLatest()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Latest" };
        await repository.CreateAsync(entity);
        var snapshotId = await snapshotService.TakeSnapshotAsync();

        // Act
        var lastId = await snapshotService.GetLastSnapshotIdAsync();

        // Assert
        Assert.Equal(snapshotId, lastId);
    }

    [Fact]
    public async Task DeleteSnapshotAsync_RemovesSnapshotCollections()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ToDelete" };
        await repository.CreateAsync(entity);
        var snapshotId = await snapshotService.TakeSnapshotAsync();

        var snapshotCollectionName =
            $"{collectionNameProvider.GetCollectionName(typeof(TestEntity))}_{snapshotId}";
        var collectionsBefore = await (
            await MongoDbService.GetEntityDatabase(true).ListCollectionNamesAsync()
        ).ToListAsync();
        Assert.Contains(snapshotCollectionName, collectionsBefore);

        // Act
        await snapshotService.DeleteSnapshotAsync(snapshotId);
        var collectionsAfter = await (
            await MongoDbService.GetEntityDatabase(true).ListCollectionNamesAsync()
        ).ToListAsync();

        // Assert
        Assert.DoesNotContain(snapshotCollectionName, collectionsAfter);
    }

    [Fact]
    public async Task TakeSnapshotIfNeededAsync_TakesSnapshot_WhenEventThresholdReached()
    {
        // Arrange
        var configuredSnapshotService = CreateSnapshotService(
            new SnapshotOptions
            {
                Enabled = true,
                Trigger = new SnapshotTriggerOptions
                {
                    Mode = SnapshotTriggerMode.EventCount,
                    EventThreshold = 5,
                    Frequency = SnapshotFrequency.Month,
                },
            }
        );

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Threshold" };
        await repository.CreateAsync(entity);

        var currentNumber = await sequenceGenerator.GetCurrentSequenceNumberAsync();

        // Act
        await configuredSnapshotService.TakeSnapshotIfNeededAsync(currentNumber + 5);
        var snapshots = await configuredSnapshotService.GetAllSnapshotsAsync();

        // Assert
        Assert.NotEmpty(snapshots);
    }

    [Fact]
    public async Task TakeSnapshotIfNeededAsync_TakesSnapshot_WhenTimeThresholdReached()
    {
        // Arrange
        var configuredSnapshotService = CreateSnapshotService(
            new SnapshotOptions
            {
                Enabled = true,
                Trigger = new SnapshotTriggerOptions
                {
                    Mode = SnapshotTriggerMode.Time,
                    EventThreshold = 1000,
                    Frequency = SnapshotFrequency.Day,
                },
            }
        );

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "TimeBased" };
        await repository.CreateAsync(entity);

        var current = await sequenceGenerator.GetCurrentSequenceNumberAsync();

        await MongoDbService
            .GetEntityDatabase(true)
            .GetCollection<SnapshotMetadata>("snapshots")
            .InsertOneAsync(
                new SnapshotMetadata
                {
                    SnapshotId = "manual-old",
                    EventNumber = current - 100,
                    Timestamp = DateTime.UtcNow.AddDays(-2),
                }
            );

        // Act
        await configuredSnapshotService.TakeSnapshotIfNeededAsync(current);
        var all = await configuredSnapshotService.GetAllSnapshotsAsync();

        // Assert
        Assert.Contains(all, x => x.SnapshotId != "manual-old");
    }

    [Fact]
    public async Task TakeSnapshotIfNeededAsync_TakesSnapshot_WhenEitherEventOrTimePasses()
    {
        // Arrange
        var configuredSnapshotService = CreateSnapshotService(
            new SnapshotOptions
            {
                Enabled = true,
                Trigger = new SnapshotTriggerOptions
                {
                    Mode = SnapshotTriggerMode.Either,
                    EventThreshold = 1,
                    Frequency = SnapshotFrequency.Day,
                },
            }
        );

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Either" };
        await repository.CreateAsync(entity);

        var current = await sequenceGenerator.GetCurrentSequenceNumberAsync();

        // Act
        await configuredSnapshotService.TakeSnapshotIfNeededAsync(current + 1);
        var allSnapshots = await configuredSnapshotService.GetAllSnapshotsAsync();

        // Assert
        Assert.True(allSnapshots.Any());
    }
    
    [Fact]
    public async Task TakeSnapshotIfNeededAsync_PrunesSnapshots_OlderThanMaxAge()
    {
        // Arrange
        var maxAgeDays = 2;
        var configuredSnapshotService = CreateSnapshotService(
            new SnapshotOptions
            {
                Enabled = true,
                Trigger = new SnapshotTriggerOptions
                {
                    Mode = SnapshotTriggerMode.EventCount,
                    EventThreshold = 1,
                    Frequency = SnapshotFrequency.Day,
                },
                Retention = new SnapshotRetentionOptions
                {
                    Strategy = SnapshotRetentionStrategy.Time,
                    MaxAgeDays = maxAgeDays,
                }
            }
        );

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "RetentionTest" };
        await repository.CreateAsync(entity);

        var now = DateTime.UtcNow;
        var currentEventNumber = await sequenceGenerator.GetCurrentSequenceNumberAsync();

        var snapshotCollection = MongoDbService
            .GetEntityDatabase(true)
            .GetCollection<SnapshotMetadata>("snapshots");

        await snapshotCollection.InsertManyAsync(new[]
        {
            new SnapshotMetadata
            {
                SnapshotId = "old-1",
                EventNumber = currentEventNumber - 10,
                Timestamp = now.AddDays(-maxAgeDays - 1)
            },
            new SnapshotMetadata
            {
                SnapshotId = "old-2",
                EventNumber = currentEventNumber - 5,
                Timestamp = now.AddDays(-maxAgeDays - 2)
            }
        });

        // Act
        await configuredSnapshotService.TakeSnapshotIfNeededAsync(currentEventNumber + 1);
        var remainingSnapshots = await configuredSnapshotService.GetAllSnapshotsAsync();

        // Assert
        Assert.DoesNotContain(remainingSnapshots, s => s.SnapshotId == "old-1");
        Assert.DoesNotContain(remainingSnapshots, s => s.SnapshotId == "old-2");
        Assert.Contains(remainingSnapshots, s => s.SnapshotId != "old-1" && s.SnapshotId != "old-2");
    }


    private SnapshotService CreateSnapshotService(SnapshotOptions options)
    {
        var wrappedOptions = Microsoft.Extensions.Options.Options.Create(
            new EventSourcingOptions { Snapshot = options }
        );

        return new SnapshotService(
            MongoDbService,
            sequenceGenerator,
            collectionNameProvider,
            new GlobalReplayContext(),
            wrappedOptions
        );
    }
}
