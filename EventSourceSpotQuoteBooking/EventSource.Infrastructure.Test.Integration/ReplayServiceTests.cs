using EventSource.Application.Interfaces;
using EventSource.Core.Interfaces;
using EventSource.Infrastructure.Interfaces;
using EventSource.Persistence.Events;
using EventSource.Persistence.Interfaces;
using EventSource.Persistence.Snapshot;
using EventSource.Test.Utilities;

namespace EventSource.Infrastructure.Test.Integration;

[Collection("Integration")]
public class ReplayServiceTests : MongoIntegrationTestBase
{
    private readonly IReplayService replayService;
    private readonly IEventStore eventStore;
    private readonly IEntityStore entityStore;
    private readonly IRepository<TestEntity> repository;
    private readonly ISnapshotService snapshotService;

    public ReplayServiceTests(
        IMongoDbService mongoDbService,
        IGlobalReplayContext replayContext,
        IReplayService replayService,
        IEventStore eventStore,
        IEntityStore entityStore,
        IRepository<TestEntity> repository,
        ISnapshotService snapshotService
    )
        : base(mongoDbService, replayContext)
    {
        this.replayService = replayService;
        this.eventStore = eventStore;
        this.entityStore = entityStore;
        this.repository = repository;
        this.snapshotService = snapshotService;
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async Task ReplayAllAsync_RehydratesCorrectly(bool autoStop, bool useSnapshot)
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "BaseSnap" };
        await repository.CreateAsync(entity);
        await snapshotService.TakeSnapshotAsync();

        entity.Name = "SnapModified";
        await eventStore.InsertEventAsync(new UpdateEvent<TestEntity>(entity));
        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayAllAsync(autoStop, useSnapshot);

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("SnapModified", result!.Name);
    }

    [Fact]
    public async Task ReplayFromUntilAsync_WithSnapshot_ReplaysInWindow()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "WindowStart" };
        await repository.CreateAsync(entity);
        await snapshotService.TakeSnapshotAsync();
        var start = DateTime.UtcNow;

        await Task.Delay(10);
        entity.Name = "WindowEnd";
        var update = new UpdateEvent<TestEntity>(entity);
        await eventStore.InsertEventAsync(update);

        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayFromUntilAsync(
            start,
            DateTime.UtcNow,
            autoStop: true,
            useSnapshot: true
        );

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("WindowEnd", result!.Name);
    }

    [Fact]
    public async Task ReplayFromUntilEventNumberAsync_WithSnapshot_RespectsRange()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "RangeBase" };
        await repository.CreateAsync(entity);
        await snapshotService.TakeSnapshotAsync();

        var update = new UpdateEvent<TestEntity>(entity) { Entity = { Name = "RangeNew" } };
        await eventStore.InsertEventAsync(update);
        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayFromUntilEventNumberAsync(
            update.EventNumber,
            update.EventNumber,
            autoStop: true,
            useSnapshot: true
        );

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("RangeNew", result!.Name);
    }

    [Fact]
    public async Task ReplayEntityAsync_AppliesEventsAfterSnapshot()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "EntitySnap" };
        await repository.CreateAsync(entity);
        await snapshotService.TakeSnapshotAsync();

        entity.Name = "EntityReplayed";
        await eventStore.InsertEventAsync(new UpdateEvent<TestEntity>(entity));
        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayEntityAsync(entity.Id, autoStop: true, useSnapshot: true);

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("EntityReplayed", result!.Name);
    }

    [Fact]
    public async Task ReplayEventAsync_Standalone_WithSnapshot()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "SoloSnap" };
        await repository.CreateAsync(entity);
        await snapshotService.TakeSnapshotAsync();

        entity.Name = "SoloModified";
        var evt = new UpdateEvent<TestEntity>(entity);
        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayEventAsync(evt, autoStop: true);

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("SoloModified", result!.Name);
    }

    [Fact]
    public async Task SnapshotFallback_ReplayAll_WhenNoSnapshotExists()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "NoSnapYet" };
        var create = new CreateEvent<TestEntity>(entity);
        await eventStore.InsertEventAsync(create);

        // Act
        await replayService.ReplayAllAsync(autoStop: true, useSnapshot: true);

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("NoSnapYet", result!.Name);
    }

    [Fact]
    public async Task ReplayUntilAsync_IgnoresEventsAfterCutoff()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Early" };
        await repository.CreateAsync(entity);
        var cutoff = DateTime.UtcNow;
        await Task.Delay(10);

        entity.Name = "TooLate";
        await eventStore.InsertEventAsync(new UpdateEvent<TestEntity>(entity));
        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayUntilAsync(cutoff, autoStop: false, useSnapshot: true);

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("Early", result!.Name);
    }

    [Fact]
    public async Task ReplayWithAutoStopAsync_ReturnsToCurrentState()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Early" };
        await repository.CreateAsync(entity);
        var cutoff = DateTime.UtcNow;
        await Task.Delay(10);

        entity.Name = "Late";
        await eventStore.InsertEventAsync(new UpdateEvent<TestEntity>(entity));
        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayUntilAsync(cutoff, autoStop: true, useSnapshot: true);

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("Late", result!.Name);
    }

    [Fact]
    public async Task ReplayFromEventNumberAsync_OnlyReplaysLaterEvents()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Initial" };
        await repository.CreateAsync(entity);
        await snapshotService.TakeSnapshotAsync();

        var update = new UpdateEvent<TestEntity>(entity) { Entity = { Name = "PostSnapshot" } };
        await eventStore.InsertEventAsync(update);
        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayFromEventNumberAsync(
            update.EventNumber,
            autoStop: false,
            useSnapshot: true
        );

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("PostSnapshot", result!.Name);
    }

    [Fact]
    public async Task ReplayEntityUntilAsync_ReplaysUpToGivenTime()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Start" };
        await repository.CreateAsync(entity);
        var until = DateTime.UtcNow;
        await Task.Delay(10);

        entity.Name = "TooLateUpdate";
        await repository.UpdateAsync(entity);

        // Act
        await replayService.ReplayEntityUntilAsync(
            entity.Id,
            until,
            autoStop: false,
            useSnapshot: true
        );

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("Start", result!.Name);
    }

    [Fact]
    public async Task ReplayEntityFromUntilAsync_RehydratesWithinTimeRange()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "StartRange" };
        await repository.CreateAsync(entity);
        var from = DateTime.UtcNow;

        await Task.Delay(10);
        entity.Name = "EndRange";
        var update = new UpdateEvent<TestEntity>(entity);
        await eventStore.InsertEventAsync(update);

        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayEntityFromUntilAsync(
            entity.Id,
            from,
            DateTime.UtcNow,
            autoStop: true,
            useSnapshot: true
        );

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("EndRange", result!.Name);
    }

    [Fact]
    public async Task ReplayUntilEventNumberAsync_StopsAtCorrectEvent()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "BeforeStop" };
        await repository.CreateAsync(entity);
        await snapshotService.TakeSnapshotAsync();

        var update = new UpdateEvent<TestEntity>(entity) { Entity = { Name = "ShouldBeApplied" } };
        await eventStore.InsertEventAsync(update);
        var stopNumber = update.EventNumber;

        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayUntilEventNumberAsync(
            stopNumber,
            autoStop: true,
            useSnapshot: true
        );

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("ShouldBeApplied", result!.Name);
    }

    [Fact]
    public async Task ReplayAllAsync_RehydratesMigratedEntity()
    {
        // Arrange
        var legacyEntity = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "Migrated",
            LastName = "Entity",
        };

        var create = new CreateEvent<TestEntity1>(legacyEntity);
        await eventStore.InsertEventAsync(create);

        // Act
        await replayService.ReplayAllAsync(autoStop: true, useSnapshot: false);

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(legacyEntity.Id);
        Assert.NotNull(result);
        Assert.Equal("Migrated - Entity", result!.Name);
        Assert.Equal(3, result.SchemaVersion); // migrated to current version
    }

    [Fact]
    public async Task ReplayAllAsync_MigratesChainedVersions()
    {
        // Arrange
        var v1 = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "Chain",
            LastName = "Test",
        };

        await eventStore.InsertEventAsync(new CreateEvent<TestEntity1>(v1));

        // Act
        await replayService.ReplayAllAsync(autoStop: true, useSnapshot: false);

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(v1.Id);
        Assert.NotNull(result);
        Assert.Equal("Chain - Test", result.Name); // migrated fully
    }

    [Fact]
    public async Task ReplayAllAsync_ProjectionWorksAfterMigration()
    {
        // Arrange
        var v1 = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "Project",
            LastName = "AfterMigration",
        };

        await eventStore.InsertEventAsync(new CreateEvent<TestEntity1>(v1));

        // Act
        await replayService.ReplayAllAsync(autoStop: true, useSnapshot: false);

        // Assert
        var projection = await entityStore.GetProjectionByFilterAsync<TestEntity, string>(
            e => e.Name == "Project - AfterMigration",
            e => e.Name
        );

        Assert.Equal("Project - AfterMigration", projection);
    }

    [Fact]
    public async Task ReplayEntityAsync_MigratesLegacyEntity()
    {
        // Arrange
        var v1 = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "Legacy",
            LastName = "One",
        };

        await eventStore.InsertEventAsync(new CreateEvent<TestEntity1>(v1));

        // Act
        await replayService.ReplayEntityAsync(v1.Id, autoStop: true, useSnapshot: false);

        // Assert
        var entity = await entityStore.GetEntityByIdAsync<TestEntity>(v1.Id);
        Assert.NotNull(entity);
        Assert.Equal("Legacy - One", entity!.Name);
    }

    [Fact]
    public async Task ReplayAllAsync_CleansPreviousReplayAndRestoresFinalState()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "CleanMe" };
        await repository.CreateAsync(entity);
        await snapshotService.TakeSnapshotAsync();

        entity.Name = "ChangedLater";
        await eventStore.InsertEventAsync(new UpdateEvent<TestEntity>(entity));

        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayAllAsync(autoStop: true, useSnapshot: true);

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("ChangedLater", result!.Name);
    }

    [Fact]
    public async Task ReplayUntilEventNumberAsync_LeavesEntityAtV2_WhenOnlyV1AndV2EventsExist()
    {
        // Arrange
        var v1 = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
        };
        await eventStore.InsertEventAsync(new CreateEvent<TestEntity1>(v1));

        var v2 = new TestEntity2
        {
            Id = v1.Id,
            FirstName = "John",
            SurName = "Doe",
            Age = 30,
        };
        var updateV2 = new UpdateEvent<TestEntity2>(v2);
        await eventStore.InsertEventAsync(updateV2);

        await eventStore.InsertEventAsync(new DeleteEvent<TestEntity2>(v2));

        // Act
        await replayService.ReplayUntilEventNumberAsync(
            updateV2.EventNumber,
            autoStop: true,
            useSnapshot: false
        );

        // Assert
        var entity = await entityStore.GetEntityByIdAsync<TestEntity>(v1.Id);
        Assert.NotNull(entity);
        Assert.Equal("John - Doe", entity!.Name);
        Assert.Equal(3, entity.SchemaVersion);
    }

    [Fact]
    public async Task ReplayFromUntilEventNumberAsync_PartialMigrationApplied()
    {
        // Arrange
        var id = Guid.NewGuid();

        var v1 = new TestEntity1
        {
            Id = id,
            FirstName = "Partial",
            LastName = "Migration",
        };
        var e1 = new CreateEvent<TestEntity1>(v1);
        await eventStore.InsertEventAsync(e1);

        var v2 = new TestEntity2
        {
            Id = id,
            FirstName = "Partial",
            SurName = "Migration",
            Age = 40,
        };
        var e2 = new UpdateEvent<TestEntity2>(v2);
        await eventStore.InsertEventAsync(e2);

        var final = new TestEntity { Id = id, Name = "Final Name" };
        var e3 = new UpdateEvent<TestEntity>(final);
        await eventStore.InsertEventAsync(e3);

        await eventStore.InsertEventAsync(new DeleteEvent<TestEntity>(final));

        // Act
        await replayService.ReplayFromUntilEventNumberAsync(
            e1.EventNumber,
            e2.EventNumber,
            autoStop: false,
            useSnapshot: false
        );

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(id);
        Assert.NotNull(result);
        Assert.Equal("Partial - Migration", result.Name);
        Assert.Equal(3, result.SchemaVersion);
    }
}
