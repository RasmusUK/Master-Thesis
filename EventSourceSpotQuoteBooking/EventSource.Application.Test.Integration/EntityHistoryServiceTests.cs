using EventSource.Application.Interfaces;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Events;
using EventSource.Test.Utilities;

namespace EventSource.Application.Integration.Test;

[Collection("Integration")]
public class EntityHistoryServiceTests
{
    private readonly IEntityHistoryService entityHistoryService;
    private readonly IEventStore eventStore;

    public EntityHistoryServiceTests(
        IEntityHistoryService entityHistoryService,
        IEventStore eventStore
    )
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

        await eventStore.InsertEventAsync(
            new CreateEvent<TestEntity>(entity1) { Entity = entity1 }
        );
        await eventStore.InsertEventAsync(
            new UpdateEvent<TestEntity>(entity2) { Entity = entity2 }
        );

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

        var deleteEvent = new DeleteEvent<TestEntity>(deleted) { Entity = deleted };
        await eventStore.InsertEventAsync(deleteEvent);

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
        Assert.Equal(deleteEvent.EntityId, returnedEvent.EntityId);
        Assert.Equal(deleteEvent.Id, returnedEvent.Id);
        Assert.Equal(deleteEvent.GetType(), returnedEvent.GetType());
    }
}
