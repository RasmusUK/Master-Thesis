using EventSource.Core.Interfaces;
using EventSource.Persistence.Events;
using EventSource.Test.Utilities;

namespace EventSource.Persistence.Test.Integration;

[Collection("Integration")]
public class EventStoreTests
{
    private readonly IEventStore eventStore;

    public EventStoreTests(IEventStore eventStore)
    {
        this.eventStore = eventStore;
    }

    [Fact]
    public async Task InsertEventAsync_StoresEventWithSequenceNumber()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var testEvent = new CreateEvent<TestEntity>(new TestEntity { Id = entityId, Name = "X" });

        // Act
        await eventStore.InsertEventAsync(testEvent);
        var result = await eventStore.GetEventByIdAsync(testEvent.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entityId, result!.EntityId);
        Assert.True(result.EventNumber > 0);
    }

    [Fact]
    public async Task InsertEventAsync_DoesNotInsertDuplicates()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var testEvent = new CreateEvent<TestEntity>(new TestEntity { Id = entityId, Name = "A" });

        // Act
        await eventStore.InsertEventAsync(testEvent);
        await eventStore.InsertEventAsync(testEvent); // duplicate
        var events = await eventStore.GetEventsByEntityIdAsync(entityId);

        // Assert
        Assert.Single(events);
    }

    [Fact]
    public async Task GetEventByIdAsync_ReturnsExpectedEvent()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var testEvent = new CreateEvent<TestEntity>(new TestEntity { Id = entityId, Name = "B" });
        await eventStore.InsertEventAsync(testEvent);

        // Act
        var result = await eventStore.GetEventByIdAsync(testEvent.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testEvent.Id, result!.Id);
    }

    [Fact]
    public async Task GetEventsFromUntilAsync_ByTimestamp_ReturnsCorrectEvents()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var testEvent = new CreateEvent<TestEntity>(
            new TestEntity { Id = entityId, Name = "TimeRange" }
        );
        await eventStore.InsertEventAsync(testEvent);

        // Act
        var from = testEvent.Timestamp.AddMinutes(-1);
        var until = testEvent.Timestamp.AddMinutes(1);
        var events = await eventStore.GetEventsFromUntilAsync(from, until);

        // Assert
        Assert.Contains(events, e => e.Id == testEvent.Id);
    }

    [Fact]
    public async Task GetEventsByEntityIdAsync_ReturnsEntityEvents()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var testEvent = new CreateEvent<TestEntity>(
            new TestEntity { Id = entityId, Name = "ByEntity" }
        );
        await eventStore.InsertEventAsync(testEvent);

        // Act
        var events = await eventStore.GetEventsByEntityIdAsync(entityId);

        // Assert
        Assert.Single(events);
        Assert.Equal(entityId, events.First().EntityId);
    }

    [Fact]
    public async Task GetEventsFromUntilAsync_BySequenceRange_ReturnsCorrectEvents()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var e1 = new CreateEvent<TestEntity>(new TestEntity { Id = entityId, Name = "One" });
        var e2 = new CreateEvent<TestEntity>(new TestEntity { Id = entityId, Name = "Two" });

        await eventStore.InsertEventAsync(e1);
        await eventStore.InsertEventAsync(e2);

        // Act
        var from = e1.EventNumber;
        var until = e2.EventNumber;
        var events = await eventStore.GetEventsFromUntilAsync(from, until);

        // Assert
        Assert.True(events.Count >= 2);
        Assert.Contains(events, e => e.Id == e1.Id);
        Assert.Contains(events, e => e.Id == e2.Id);
    }

    [Fact]
    public async Task GetEventsAsync_ReturnsAllInsertedEvents()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var testEvent = new CreateEvent<TestEntity>(
            new TestEntity { Id = entityId, Name = "Global" }
        );
        await eventStore.InsertEventAsync(testEvent);

        // Act
        var allEvents = await eventStore.GetEventsAsync();

        // Assert
        Assert.Contains(allEvents, e => e.Id == testEvent.Id);
    }
}
