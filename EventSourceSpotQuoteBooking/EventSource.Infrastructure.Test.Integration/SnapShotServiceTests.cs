using EventSource.Application.Interfaces;
using EventSource.Core.Interfaces;
using EventSource.Infrastructure.Interfaces;
using EventSource.Persistence.Interfaces;
using EventSource.Test.Utilities;
using MongoDB.Driver;

namespace EventSource.Infrastructure.Test.Integration;

[Collection("Integration")]
public class SnapshotServiceTests : MongoIntegrationTestBase
{
    private readonly ISnapshotService snapshotService;
    private readonly IMongoDbService mongoDbService;
    private readonly IEntityCollectionNameProvider collectionNameProvider;
    private readonly IRepository<TestEntity> repository;

    public SnapshotServiceTests(
        IMongoDbService mongoDbService,
        IGlobalReplayContext replayContext,
        ISnapshotService snapshotService,
        IEntityCollectionNameProvider collectionNameProvider,
        IRepository<TestEntity> repository
    )
        : base(mongoDbService, replayContext)
    {
        this.mongoDbService = mongoDbService;
        this.snapshotService = snapshotService;
        this.collectionNameProvider = collectionNameProvider;
        this.repository = repository;
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
        var collection = mongoDbService.GetEntityCollection<TestEntity>(
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
            await mongoDbService.GetEntityDatabase(true).ListCollectionNamesAsync()
        ).ToListAsync();
        Assert.Contains(snapshotCollectionName, collectionsBefore);

        // Act
        await snapshotService.DeleteSnapshotAsync(snapshotId);
        var collectionsAfter = await (
            await mongoDbService.GetEntityDatabase(true).ListCollectionNamesAsync()
        ).ToListAsync();

        // Assert
        Assert.DoesNotContain(snapshotCollectionName, collectionsAfter);
    }
}
