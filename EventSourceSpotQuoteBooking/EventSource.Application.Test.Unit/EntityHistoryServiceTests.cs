using EventSource.Core.Events;
using EventSource.Core.Interfaces;
using EventSource.Test.Utilities;
using Moq;

namespace EventSource.Application.Test;

public class EntityHistoryServiceTests
{
    private readonly Mock<IEventStore> mockEventStore;
    private readonly EntityHistoryService service;

    public EntityHistoryServiceTests()
    {
        mockEventStore = new Mock<IEventStore>();
        service = new EntityHistoryService(mockEventStore.Object);
    }

    [Fact]
    public async Task GetEntityHistoryAsync_ReturnsEntitiesFromCreateUpdateDeleteEvents()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        var entityV1 = new TestEntity { Id = entityId, Name = "Created" };
        var entityV2 = new TestEntity { Id = entityId, Name = "Updated" };
        var entityV3 = new TestEntity { Id = entityId, Name = "Deleted" };

        var events = new List<IEvent>
        {
            new CreateEvent<TestEntity> { Entity = entityV1 },
            new UpdateEvent<TestEntity> { Entity = entityV2 },
            new DeleteEvent<TestEntity> { Entity = entityV3 },
        };

        mockEventStore.Setup(es => es.GetEventsByEntityIdAsync(entityId)).ReturnsAsync(events);

        // Act
        var history = await service.GetEntityHistoryAsync<TestEntity>(entityId);

        // Assert
        Assert.Equal(3, history.Count);
        Assert.Contains(history, e => e.Name == "Created");
        Assert.Contains(history, e => e.Name == "Updated");
        Assert.Contains(history, e => e.Name == "Deleted");
    }

    [Fact]
    public async Task GetEntityHistoryWithEventsAsync_ReturnsCorrectEntityEventPairs()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        var entityV1 = new TestEntity { Id = entityId, Name = "V1" };
        var entityV2 = new TestEntity { Id = entityId, Name = "V2" };

        var createEvent = new CreateEvent<TestEntity> { Entity = entityV1 };
        var updateEvent = new UpdateEvent<TestEntity> { Entity = entityV2 };

        var events = new List<IEvent> { createEvent, updateEvent };

        mockEventStore.Setup(es => es.GetEventsByEntityIdAsync(entityId)).ReturnsAsync(events);

        // Act
        var result = await service.GetEntityHistoryWithEventsAsync<TestEntity>(entityId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, pair => pair.entity.Name == "V1" && pair.e == createEvent);
        Assert.Contains(result, pair => pair.entity.Name == "V2" && pair.e == updateEvent);
    }
}
