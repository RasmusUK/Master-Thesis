using EventSource.Application;
using EventSource.Application.Interfaces;
using EventSource.Core.Interfaces;
using EventSource.Persistence;
using EventSource.Persistence.Entities;
using EventSource.Persistence.Interfaces;
using EventSource.Persistence.Stores;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EventSource.Core.Test;

public class UnitTest1 : IDisposable
{
    private readonly IEventProcessor eventProcessor;
    private readonly IEntityStore entityStore;
    private readonly IEventStore eventStore;
    private readonly IEntityHistoryService entityHistoryService;
    private readonly IReplayService replayService;
    private readonly IMongoDbService mongoDbService;

    public UnitTest1(
        IEventProcessor eventProcessor,
        IEntityStore entityStore,
        IEventStore eventStore,
        IEntityHistoryService entityHistoryService,
        IReplayService replayService,
        IMongoDbService mongoDbService
    )
    {
        this.eventProcessor = eventProcessor;
        this.entityStore = entityStore;
        this.eventStore = eventStore;
        this.entityHistoryService = entityHistoryService;
        this.replayService = replayService;
        this.mongoDbService = mongoDbService;
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
        eventProcessor.RegisterEventToEntity<CreateBookingEvent, Booking>();
        await eventProcessor.ProcessAsync(createBookingEvent);
        var events = await eventStore.GetEventsAsync();
        Assert.Single(events);
        Assert.Equal(createBookingEvent.Id, events.First().Id);

        var booking = await entityStore.GetEntityAsync<Booking>(createBookingEvent.EntityId);
        Assert.NotNull(booking);
        Assert.Equal(createBookingEvent.EntityId, booking.Id);

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
            createBookingEvent.EntityId,
            new Address("fromUpdated", "toUpdated", "zipUpdated", "zipcodeUpdated"),
            new Address("streetUpdated", "cityUpdated", "zipUpdated", "zipcodeUpdated")
        );

        eventProcessor.RegisterEventToEntity<CreateBookingEvent, Booking>();
        eventProcessor.RegisterEventToEntity<UpdateBookingAddressEvent, Booking>();
        await eventProcessor.ProcessAsync(createBookingEvent);
        await eventProcessor.ProcessAsync(updateBookingAddressEvent);

        var events = await eventStore.GetEventsAsync();
        Assert.Equal(2, events.Count);

        var booking = await entityStore.GetEntityAsync<Booking>(createBookingEvent.EntityId);
        Assert.NotNull(booking);
        Assert.Equal(createBookingEvent.EntityId, booking.Id);

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
            createBookingEvent.EntityId,
            new Address("fromUpdated", "toUpdated", "zipUpdated", "zipcodeUpdated"),
            new Address("streetUpdated", "cityUpdated", "zipUpdated", "zipcodeUpdated")
        );

        eventProcessor.RegisterEventToEntity<CreateBookingEvent, Booking>();
        eventProcessor.RegisterEventToEntity<UpdateBookingAddressEvent, Booking>();
        await eventProcessor.ProcessAsync(createBookingEvent);
        await eventProcessor.ProcessAsync(updateBookingAddressEvent);

        var bookingHistory = await entityHistoryService.GetEntityHistoryAsync<Booking>(
            createBookingEvent.EntityId
        );

        Assert.Equal(2, bookingHistory.Count);
        Assert.Equal(createBookingEvent.From, bookingHistory.First().GetFrom());
        Assert.Equal(updateBookingAddressEvent.From, bookingHistory.Last().GetFrom());
    }

    [Fact]
    public async Task Test4()
    {
        var createBookingEvent = new CreateBookingEvent(
            Guid.NewGuid(),
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var updateBookingAddressEvent = new UpdateBookingAddressEvent(
            createBookingEvent.EntityId,
            new Address("fromUpdated", "toUpdated", "zipUpdated", "zipcodeUpdated"),
            new Address("streetUpdated", "cityUpdated", "zipUpdated", "zipcodeUpdated")
        );

        eventProcessor.RegisterEventToEntity<CreateBookingEvent, Booking>();
        eventProcessor.RegisterEventToEntity<UpdateBookingAddressEvent, Booking>();
        await eventProcessor.ProcessAsync(createBookingEvent);
        await eventProcessor.ProcessAsync(updateBookingAddressEvent);

        await entityHistoryService.GetEntityHistoryAsync<Booking>(createBookingEvent.EntityId);

        var booking = await entityStore.GetEntityAsync<Booking>(createBookingEvent.EntityId);
        Assert.NotNull(booking);
        Assert.Equal(createBookingEvent.EntityId, booking.Id);

        Assert.Equal(updateBookingAddressEvent.From, booking.GetFrom());
    }

    [Fact]
    public async Task Test5()
    {
        var createBookingEvent = new CreateBookingEvent(
            Guid.NewGuid(),
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var createBookingEventHandler = new CreateBookingEventHandler(entityStore);
        eventProcessor.RegisterEventToEntity<CreateBookingEvent, Booking>();
        eventProcessor.RegisterEventHandler(createBookingEventHandler);
        await eventProcessor.ProcessAsync(createBookingEvent);

        var booking = await entityStore.GetEntityAsync<Booking>(createBookingEvent.EntityId);
        Assert.Equal("Booking", booking.Name);
    }

    [Fact]
    public async Task Test6()
    {
        var createBookingEvent = new CreateBookingEvent(
            Guid.NewGuid(),
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var updateBookingAddressEvent = new UpdateBookingAddressEvent(
            createBookingEvent.EntityId,
            new Address("fromUpdated", "toUpdated", "zipUpdated", "zipcodeUpdated"),
            new Address("streetUpdated", "cityUpdated", "zipUpdated", "zipcodeUpdated")
        );

        eventProcessor.RegisterEventToEntity<CreateBookingEvent, Booking>();
        eventProcessor.RegisterEventToEntity<UpdateBookingAddressEvent, Booking>();
        await eventProcessor.ProcessAsync(createBookingEvent);
        await eventProcessor.ProcessAsync(updateBookingAddressEvent);

        await replayService.ReplayAllEventsAsync();

        var booking = await entityStore.GetEntityAsync<Booking>(createBookingEvent.EntityId);
        Assert.Equal(updateBookingAddressEvent.From, booking.GetFrom());
    }

    [Fact]
    public async Task Test7()
    {
        var createBookingEvent = new CreateBookingEvent(
            Guid.NewGuid(),
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var updateBookingAddressEvent = new UpdateBookingAddressEvent(
            createBookingEvent.EntityId,
            new Address("fromUpdated", "toUpdated", "zipUpdated", "zipcodeUpdated"),
            new Address("streetUpdated", "cityUpdated", "zipUpdated", "zipcodeUpdated")
        );

        eventProcessor.RegisterEventToEntity<CreateBookingEvent, Booking>();
        eventProcessor.RegisterEventToEntity<UpdateBookingAddressEvent, Booking>();
        await eventProcessor.ProcessAsync(createBookingEvent);
        await eventProcessor.ProcessAsync(updateBookingAddressEvent);

        await replayService.ReplayEventAsync(createBookingEvent);

        var booking = await entityStore.GetEntityAsync<Booking>(createBookingEvent.EntityId);
        Assert.Equal(createBookingEvent.From, booking.GetFrom());
    }

    [Fact]
    public async Task Test8()
    {
        var createBookingEvent = new CreateBookingEvent(
            Guid.NewGuid(),
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var addCustomerToBookingEvent = new AddCustomerToBookingEvent(
            createBookingEvent.EntityId,
            "CustomerName"
        );

        eventProcessor.RegisterEventToEntity<CreateBookingEvent, Booking>();
        eventProcessor.RegisterEventToEntity<AddCustomerToBookingEvent, Booking>();
        await eventProcessor.ProcessAsync(createBookingEvent);
        await eventProcessor.ProcessAsync(addCustomerToBookingEvent);

        var booking = await entityStore.GetEntityAsync<Booking>(createBookingEvent.EntityId);
        Assert.Equal("CustomerName", booking.CustomerName);
    }

    [Fact]
    public async Task Test9()
    {
        var createBookingEvent = new CreateBookingEvent(
            Guid.NewGuid(),
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var addCustomerToBookingEvent = new AddCustomerToBookingEvent(
            createBookingEvent.EntityId,
            "CustomerName"
        );

        eventProcessor.RegisterEventToEntity<CreateBookingEvent, Booking>();
        eventProcessor.RegisterEventToEntity<AddCustomerToBookingEvent, Booking>();
        await eventProcessor.ProcessAsync(createBookingEvent);
        await eventProcessor.ProcessAsync(addCustomerToBookingEvent);

        var events = await eventStore.GetEventsAsync();
        Assert.Null(((AddCustomerToBookingEvent)events.Last()).CustomerName);
    }

    public void Dispose()
    {
        mongoDbService.EventCollection.DeleteMany(Builders<MongoDbEvent>.Filter.Empty);
        mongoDbService.EntityCollection.DeleteMany(Builders<MongoDbEntity>.Filter.Empty);
        mongoDbService.PersonalDataCollection.DeleteMany(
            Builders<MongoDbPersonalData>.Filter.Empty
        );
    }
}
