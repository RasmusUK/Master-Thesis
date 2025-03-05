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
    private readonly IEntityHistoryStore entityHistoryStore;
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
        entityStore = new MongoDbEntityStore(mongoDbService);
        var eventHandler = new EventHandler(entityStore);
        eventProcessor = new EventProcessor(eventStore, eventHandler);
        entityHistoryStore = new MongoDbEntityHistoryStore(eventStore, entityStore, eventProcessor);
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

    [Fact]
    public async Task Test2()
    {
        var createBookingEvent = new CreateBookingEvent(
            Guid.NewGuid(),
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var updateBookingAddressEvent = new UpdateBookingAddressEvent(
            createBookingEvent.AggregateId,
            new Address("fromUpdated", "toUpdated", "zipUpdated", "zipcodeUpdated"),
            new Address("streetUpdated", "cityUpdated", "zipUpdated", "zipcodeUpdated")
        );

        eventProcessor.RegisterHandler<CreateBookingEvent, Booking>();
        eventProcessor.RegisterHandler<UpdateBookingAddressEvent, Booking>();
        await eventProcessor.ProcessAsync(createBookingEvent);
        await eventProcessor.ProcessAsync(updateBookingAddressEvent);

        var events = await eventStore.GetEventsAsync();
        Assert.Equal(2, events.Count);

        var booking = await entityStore.GetEntityAsync<Booking>(createBookingEvent.AggregateId);
        Assert.NotNull(booking);
        Assert.Equal(createBookingEvent.AggregateId, booking.Id);

        Assert.Equal(updateBookingAddressEvent.From, booking.GetFrom());
    }

    [Fact]
    public async Task Test3()
    {
        var createBookingEvent = new CreateBookingEvent(
            Guid.NewGuid(),
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var updateBookingAddressEvent = new UpdateBookingAddressEvent(
            createBookingEvent.AggregateId,
            new Address("fromUpdated", "toUpdated", "zipUpdated", "zipcodeUpdated"),
            new Address("streetUpdated", "cityUpdated", "zipUpdated", "zipcodeUpdated")
        );

        eventProcessor.RegisterHandler<CreateBookingEvent, Booking>();
        eventProcessor.RegisterHandler<UpdateBookingAddressEvent, Booking>();
        await eventProcessor.ProcessAsync(createBookingEvent);
        await eventProcessor.ProcessAsync(updateBookingAddressEvent);

        var bookingHistory = await entityHistoryStore.GetEntityHistoryAsync<Booking>(
            createBookingEvent.AggregateId
        );

        Assert.Equal(2, bookingHistory.Count);
        Assert.Equal(createBookingEvent.From, bookingHistory.First().GetFrom());
        Assert.Equal(updateBookingAddressEvent.From, bookingHistory.Last().GetFrom());
    }

    public void Dispose()
    {
        eventCollection.DeleteMany(Builders<MongoDbEvent>.Filter.Empty);
        aggregateRootCollection.DeleteMany(Builders<MongoDbEntity>.Filter.Empty);
    }
}
