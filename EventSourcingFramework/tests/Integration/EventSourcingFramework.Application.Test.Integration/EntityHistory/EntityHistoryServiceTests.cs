using EventSourcingFramework.Application.Abstractions.EntityHistory;
using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Models.Events;
using EventSourcingFramework.Test.Utilities;
using EventSourcingFramework.Test.Utilities.Models;

namespace EventSourcingFramework.Application.Test.Integration.EntityHistory;

[Collection("Integration")]
public class EntityHistoryServiceTests : MongoIntegrationTestBase
{
    private readonly IEntityHistoryService entityHistoryService;
    private readonly IEventStore eventStore;

    public EntityHistoryServiceTests(
        IMongoDbService mongoDbService,
        IGlobalReplayContext replayContext,
        IEntityHistoryService entityHistoryService,
        IEventStore eventStore
    )
        : base(mongoDbService, replayContext)
    {
        this.entityHistoryService = entityHistoryService;
        this.eventStore = eventStore;
    }

    [Fact]
    public async Task GetEntityHistoryAsync_ReturnsInsertedEvents()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity1 = new TestEntity { Id = entityId, Name = "Created" };
        var entity2 = new TestEntity { Id = entityId, Name = "Updated" };

        await eventStore.InsertEventAsync(new MongoCreateEvent<TestEntity>(entity1));
        await eventStore.InsertEventAsync(new MongoUpdateEvent<TestEntity>(entity2));

        // Act
        var result = await entityHistoryService.GetEntityHistoryAsync<TestEntity>(entityId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.Name == "Created");
        Assert.Contains(result, e => e.Name == "Updated");
    }

    [Fact]
    public async Task GetEntityHistoryWithEventsAsync_ReturnsCorrectEntityEventPairs()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var deleted = new TestEntity { Id = entityId, Name = "Deleted" };

        var MongoDeleteEvent = new MongoDeleteEvent<TestEntity>(deleted);
        await eventStore.InsertEventAsync(MongoDeleteEvent);

        // Act
        var result = await entityHistoryService.GetEntityHistoryWithEventsAsync<TestEntity>(
            entityId
        );

        // Assert
        Assert.Single(result);
        var (returnedEntity, returnedEvent) = result.First();

        // For entity
        Assert.Equal("Deleted", returnedEntity.Name);
        Assert.Equal(deleted.Id, returnedEntity.Id);

        // For event
        Assert.Equal(MongoDeleteEvent.EntityId, returnedEvent.EntityId);
        Assert.Equal(MongoDeleteEvent.Id, returnedEvent.Id);
        Assert.Equal(MongoDeleteEvent.GetType(), returnedEvent.GetType());
    }
}