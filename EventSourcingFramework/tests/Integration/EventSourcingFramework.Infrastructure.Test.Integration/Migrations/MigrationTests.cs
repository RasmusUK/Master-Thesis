using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Models.Events;
using EventSourcingFramework.Test.Utilities;
using EventSourcingFramework.Test.Utilities.Models;

namespace EventSourcingFramework.Infrastructure.Test.Integration.Migrations;

[Collection("Integration")]
public class MigrationTests : MongoIntegrationTestBase
{
    private readonly IEventStore eventStore;
    private readonly IEntityStore entityStore;
    
    public MigrationTests(IMongoDbService mongoDbService, IReplayContext replayContext, IEventStore eventStore, IEntityStore entityStore) : base(mongoDbService, replayContext)
    {
        this.eventStore = eventStore;
        this.entityStore = entityStore;
    }

    [Fact]
    public async Task OldEvents_ReturnsWithoutMigrationAsync()
    {
        // Arrange
        var oldEvent = new CreateEvent<TestEntity1>(new TestEntity1
        {
            FirstName = "John",
            LastName = "Doe"
        });

        await eventStore.InsertEventAsync(oldEvent);

        // Act
        var events = await eventStore.GetEventsByEntityIdAsync(oldEvent.EntityId);

        // Assert
        Assert.Single(events);

        var e = events.First();
        Assert.IsType<CreateEvent<TestEntity1>>(e);

        var typedEvent = (CreateEvent<TestEntity1>)e;
        var entity = typedEvent.Entity;

        Assert.IsType<TestEntity1>(entity);
        Assert.Equal("John", entity.FirstName);
        Assert.Equal("Doe", entity.LastName);
    }
}