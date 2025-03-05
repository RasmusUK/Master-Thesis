using EventSource.Application;
using EventSource.Core.Interfaces;
using EventSource.Persistence;
using EventSource.Persistence.Entities;
using EventSource.Persistence.Stores;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using EventHandler = EventSource.Application.EventHandler;

namespace EventSource.Core.Test;

public class UnitTest1 : IDisposable
{
    private readonly IEventProcessor eventProcessor;
    private readonly IEntityStore entityStore;
    private readonly IEventStore eventStore;
    private readonly IMongoCollection<MongoDbEvent> eventCollection;
    private readonly IMongoCollection<MongoDbEntity> aggregateRootCollection;

    public UnitTest1()
    {
        var mongoDbOptions = new MongoDbOptions
        {
            EventStore = new EventStoreOptions
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "EventSource",
            },
        };
        var mongoDbIOptions = Options.Create(mongoDbOptions);
        var mongoDbService = new MongoDbService(mongoDbIOptions);
        eventCollection = mongoDbService.EventCollection;
        aggregateRootCollection = mongoDbService.AggregateRootCollection;
        eventStore = new MongoDbEventStore(mongoDbService);
        entityStore = new EntityStore(mongoDbService);
        var eventHandler = new EventHandler(entityStore);
        eventProcessor = new EventProcessor(eventStore, eventHandler);
        Dispose();
    }

    [Fact]
    public async Task Test1()
    {
        var createBookingEvent = new CreateBookingEvent(
            Guid.NewGuid(),
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );
        eventProcessor.RegisterHandler<CreateBookingEvent, Booking>();
        await eventProcessor.ProcessAsync(createBookingEvent);
        var events = await eventStore.GetEventsAsync();
        Assert.Single(events);
        Assert.Equal(createBookingEvent.Id, events.First().Id);

        var booking = await entityStore.GetEntityAsync<Booking>(createBookingEvent.AggregateId);
        Assert.NotNull(booking);
        Assert.Equal(createBookingEvent.AggregateId, booking.Id);

        Assert.NotNull(booking.GetFrom());
        Assert.Equal("from", booking.GetFrom().Street);
        Assert.Equal("Country", booking.GetFrom().GetCountry());
    }

    public void Dispose()
    {
        eventCollection.DeleteMany(Builders<MongoDbEvent>.Filter.Empty);
        aggregateRootCollection.DeleteMany(Builders<MongoDbEntity>.Filter.Empty);
    }
}
