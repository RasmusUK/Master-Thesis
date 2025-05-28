using EventSourcingFramework.Application.Abstractions.Replay;
using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Application.Abstractions.Snapshots;
using EventSourcingFramework.Application.UseCases.Replay;
using EventSourcingFramework.Core.Enums;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Configuration.Options;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Models.Events;
using EventSourcingFramework.Infrastructure.Snapshots.Config;
using EventSourcingFramework.Test.Utilities;
using EventSourcingFramework.Test.Utilities.Models;
using Microsoft.Extensions.Options;

namespace EventSourcingFramework.Application.Test.Integration.Replay;

[Collection("Integration")]
public class ReplayServiceTests : MongoIntegrationTestBase
{
    private readonly IEntityStore entityStore;
    private readonly IEventStore eventStore;
    private readonly IReplayEnvironmentSwitcher replayEnvironmentSwitcher;
    private readonly IReplayService replayService;
    private readonly IRepository<TestEntity> repository;
    private readonly ISnapshotService snapshotService;

    public ReplayServiceTests(
        IMongoDbService mongoDbService,
        IReplayContext replayContext,
        IReplayService replayService,
        IEventStore eventStore,
        IEntityStore entityStore,
        IRepository<TestEntity> repository,
        ISnapshotService snapshotService, IReplayEnvironmentSwitcher replayEnvironmentSwitcher)
        : base(mongoDbService, replayContext)
    {
        this.replayService = replayService;
        this.eventStore = eventStore;
        this.entityStore = entityStore;
        this.repository = repository;
        this.snapshotService = snapshotService;
        this.replayEnvironmentSwitcher = replayEnvironmentSwitcher;
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
        await snapshotService.TakeSnapshotAsync(1);

        entity.Name = "SnapModified";
        await eventStore.InsertEventAsync(new MongoUpdateEvent<TestEntity>(entity));
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
        await snapshotService.TakeSnapshotAsync(1);
        var start = DateTime.UtcNow;

        await Task.Delay(10);
        entity.Name = "WindowEnd";
        var update = new MongoUpdateEvent<TestEntity>(entity);
        await eventStore.InsertEventAsync(update);

        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayFromUntilAsync(
            start,
            DateTime.UtcNow,
            true,
            true
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
        await snapshotService.TakeSnapshotAsync(1);

        var update = new MongoUpdateEvent<TestEntity>(entity) { Entity = { Name = "RangeNew" } };
        await eventStore.InsertEventAsync(update);
        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayFromUntilEventNumberAsync(
            update.EventNumber,
            update.EventNumber,
            true,
            true
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
        await snapshotService.TakeSnapshotAsync(2);

        entity.Name = "EntityReplayed";
        await eventStore.InsertEventAsync(new MongoUpdateEvent<TestEntity>(entity));
        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayEntityAsync(entity.Id, true, true);

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
        await snapshotService.TakeSnapshotAsync(1);

        entity.Name = "SoloModified";
        var evt = new MongoUpdateEvent<TestEntity>(entity);
        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayEventAsync(evt, true);

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
        await eventStore.InsertEventAsync(new MongoCreateEvent<TestEntity>(entity));
        await eventStore.InsertEventAsync(new MongoDeleteEvent<TestEntity>(entity));
        await eventStore.InsertEventAsync(new MongoCreateEvent<TestEntity>(entity));

        // Act
        await replayService.ReplayAllAsync(true, true);

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
        await eventStore.InsertEventAsync(new MongoUpdateEvent<TestEntity>(entity));
        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayUntilAsync(cutoff, false, true);

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
        await eventStore.InsertEventAsync(new MongoUpdateEvent<TestEntity>(entity));
        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayUntilAsync(cutoff, true, true);

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
        await snapshotService.TakeSnapshotAsync(1);

        var update = new MongoUpdateEvent<TestEntity>(entity) { Entity = { Name = "PostSnapshot" } };
        await eventStore.InsertEventAsync(update);
        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayFromEventNumberAsync(
            update.EventNumber,
            false,
            true
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
            false,
            true
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
        var update = new MongoUpdateEvent<TestEntity>(entity);
        await eventStore.InsertEventAsync(update);

        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayEntityFromUntilAsync(
            entity.Id,
            from,
            DateTime.UtcNow,
            true,
            true
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
        await snapshotService.TakeSnapshotAsync(1);

        var update = new MongoUpdateEvent<TestEntity>(entity) { Entity = { Name = "ShouldBeApplied" } };
        await eventStore.InsertEventAsync(update);
        var stopNumber = update.EventNumber;

        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayUntilEventNumberAsync(
            stopNumber,
            true,
            true
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
            LastName = "Entity"
        };

        var create = new MongoCreateEvent<TestEntity1>(legacyEntity);
        await eventStore.InsertEventAsync(create);

        // Act
        await replayService.ReplayAllAsync(true, false);

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
            LastName = "Test"
        };

        await eventStore.InsertEventAsync(new MongoCreateEvent<TestEntity1>(v1));

        // Act
        await replayService.ReplayAllAsync(true, false);

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
            LastName = "AfterMigration"
        };

        await eventStore.InsertEventAsync(new MongoCreateEvent<TestEntity1>(v1));

        // Act
        await replayService.ReplayAllAsync(true, false);

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
            LastName = "One"
        };

        await eventStore.InsertEventAsync(new MongoCreateEvent<TestEntity1>(v1));

        // Act
        await replayService.ReplayEntityAsync(v1.Id, true, false);

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
        await snapshotService.TakeSnapshotAsync(1);

        entity.Name = "ChangedLater";
        await eventStore.InsertEventAsync(new MongoUpdateEvent<TestEntity>(entity));

        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayAllAsync(true, true);

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
            LastName = "Doe"
        };
        await eventStore.InsertEventAsync(new MongoCreateEvent<TestEntity1>(v1));

        var v2 = new TestEntity2
        {
            Id = v1.Id,
            FirstName = "John",
            SurName = "Doe",
            Age = 30
        };
        var updateV2 = new MongoUpdateEvent<TestEntity2>(v2);
        await eventStore.InsertEventAsync(updateV2);

        await eventStore.InsertEventAsync(new MongoDeleteEvent<TestEntity2>(v2));

        // Act
        await replayService.ReplayUntilEventNumberAsync(
            updateV2.EventNumber,
            false,
            false
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
            LastName = "Migration"
        };
        var e1 = new MongoCreateEvent<TestEntity1>(v1);
        await eventStore.InsertEventAsync(e1);

        var v2 = new TestEntity2
        {
            Id = id,
            FirstName = "Partial",
            SurName = "Migration",
            Age = 40
        };
        var e2 = new MongoUpdateEvent<TestEntity2>(v2);
        await eventStore.InsertEventAsync(e2);

        var final = new TestEntity { Id = id, Name = "Final Name" };
        var e3 = new MongoUpdateEvent<TestEntity>(final);
        await eventStore.InsertEventAsync(e3);

        await eventStore.InsertEventAsync(new MongoDeleteEvent<TestEntity>(final));

        // Act
        await replayService.ReplayFromUntilEventNumberAsync(
            e1.EventNumber,
            e2.EventNumber,
            false,
            false
        );

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(id);
        Assert.NotNull(result);
        Assert.Equal("Partial - Migration", result.Name);
        Assert.Equal(3, result.SchemaVersion);
    }

    [Fact]
    public async Task ReplayService_DebugMode_CapturesSimulatedEventsAndSwitchesDatabases()
    {
        // Arrange
        await replayService.StartReplay(ReplayMode.Debug);

        var id = Guid.NewGuid();
        var entity = new TestEntity { Id = id, Name = "DebugSimulated" };

        await repository.CreateAsync(entity);
        await repository.UpdateAsync(entity);
        await repository.DeleteAsync(entity);

        // Act
        var simulated = replayService.GetSimulatedEvents();

        // Assert
        Assert.NotNull(simulated);
        Assert.Equal(3, simulated.Count);
        Assert.Contains(simulated, e => e is MongoCreateEvent<TestEntity>);
        Assert.Contains(simulated, e => e is MongoUpdateEvent<TestEntity>);
        Assert.Contains(simulated, e => e is MongoDeleteEvent<TestEntity>);
    }

    [Fact]
    public async Task StartReplay_SetsReplayModeToRunning()
    {
        // Act
        await replayService.StartReplay();

        // Assert
        Assert.True(replayService.IsRunning());
    }

    [Fact]
    public async Task ReplayFromAsync_RehydratesEntityStateFromGivenTimestamp()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "BeforeReplay" };
        await repository.CreateAsync(entity);

        var from = DateTime.UtcNow;
        await Task.Delay(10);

        entity.Name = "AfterReplay";
        await eventStore.InsertEventAsync(new MongoUpdateEvent<TestEntity>(entity));
        await entityStore.DeleteEntityAsync(entity);

        // Act
        await replayService.ReplayFromAsync(from, true, false);

        // Assert
        var result = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("AfterReplay", result!.Name);
    }

    [Fact]
    public async Task ReplayService_DoesNotUseSnapshots_WhenSnapshotsDisabled()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var entity = new TestEntity { Id = testId, Name = "NoSnapshots" };
        await repository.CreateAsync(entity);

        var replayServiceWithSnapshotsDisabled = new ReplayService(
            eventStore,
            entityStore,
            ReplayContext,
            snapshotService,
            replayEnvironmentSwitcher,
            new SnapshotSettingsAdapter(
                Options.Create(new EventSourcingOptions
                {
                    Snapshot = new SnapshotOptions
                    {
                        Enabled = false
                    }
                }))
        );

        // Act
        await replayServiceWithSnapshotsDisabled.ReplayAllAsync(useSnapshot: true);
        await replayServiceWithSnapshotsDisabled.ReplayFromAsync(DateTime.UtcNow, useSnapshot: true);
        await replayServiceWithSnapshotsDisabled.ReplayFromEventNumberAsync(1, useSnapshot: true);
        await replayServiceWithSnapshotsDisabled.ReplayUntilEventNumberAsync(10000, useSnapshot: true);
        await replayServiceWithSnapshotsDisabled.ReplayFromUntilEventNumberAsync(1, 10000, useSnapshot: true);

        // Assert
        var snapshots = await snapshotService.GetAllSnapshotsAsync();
        Assert.Empty(snapshots);
    }

    [Fact]
    public async Task StopReplay_SetsReplayToNotRunning()
    {
        // Arrange
        await replayService.StartReplay(ReplayMode.Debug);

        // Act
        await replayService.StopReplay();

        // Assert
        Assert.False(replayService.IsRunning());
    }
}