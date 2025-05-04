using EventSource.Application.Interfaces;
using EventSource.Core.Interfaces;
using EventSource.Infrastructure.Interfaces;
using EventSource.Persistence.Events;
using EventSource.Persistence.Interfaces;
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
}
