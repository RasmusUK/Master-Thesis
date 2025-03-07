using EventSource.Application.Interfaces;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Entities;
using EventSource.Persistence.Interfaces;
using EventSource.Persistence.Stores;
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
    private readonly IEntityStore mongoDbEntityStore;

    public UnitTest1(
        IEventProcessor eventProcessor,
        IEntityStore entityStore,
        IEventStore eventStore,
        IEntityHistoryService entityHistoryService,
        IReplayService replayService,
        IMongoDbService mongoDbService,
        IEntityStore mongoDbEntityStore
    )
    {
        this.eventProcessor = eventProcessor;
        this.entityStore = entityStore;
        this.eventStore = eventStore;
        this.entityHistoryService = entityHistoryService;
        this.replayService = replayService;
        this.mongoDbService = mongoDbService;
        this.mongoDbEntityStore = mongoDbEntityStore;
        eventProcessor.RegisterEventToEntity<CreateBookingEvent, Booking>();
        eventProcessor.RegisterEventToEntity<AddCustomerToBookingEvent, Booking>();
        eventProcessor.RegisterEventToEntity<UpdateBookingAddressEvent, Booking>();
        Dispose();
    }

    [Fact]
    public async Task Test1()
    {
        var createBookingEvent = new CreateBookingEvent(
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );
        eventProcessor.RegisterEventToEntity<CreateBookingEvent, Booking>();
        var booking = await eventProcessor.ProcessAsync(createBookingEvent);
        var events = await eventStore.GetEventsAsync();
        Assert.Single(events);
        Assert.Equal(createBookingEvent.Id, events.First().Id);

        var fetchedBooking = await entityStore.GetEntityByIdAsync<Booking>(booking.Id);
        Assert.NotNull(fetchedBooking);
        Assert.Equal(booking.Id, fetchedBooking.Id);

        Assert.NotNull(fetchedBooking.From);
        Assert.Equal("from", fetchedBooking.From.Street);
        Assert.Equal("Country", fetchedBooking.From.GetCountry());
    }

    [Fact]
    public async Task Test2()
    {
        var createBookingEvent = new CreateBookingEvent(
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var booking = await eventProcessor.ProcessAsync(createBookingEvent);
        var updateBookingAddressEvent = new UpdateBookingAddressEvent(
            booking.Id,
            new Address("fromUpdated", "toUpdated", "zipUpdated", "zipcodeUpdated"),
            new Address("streetUpdated", "cityUpdated", "zipUpdated", "zipcodeUpdated")
        );

        booking = await eventProcessor.ProcessAsync(updateBookingAddressEvent);

        var events = await eventStore.GetEventsAsync();
        Assert.Equal(2, events.Count);

        var fetchedBooking = await entityStore.GetEntityByIdAsync<Booking>(booking.Id);
        Assert.NotNull(fetchedBooking);
        Assert.Equal(createBookingEvent.EntityId, fetchedBooking.Id);

        Assert.Equal(updateBookingAddressEvent.From, fetchedBooking.From);
    }

    [Fact]
    public async Task Test3()
    {
        var createBookingEvent = new CreateBookingEvent(
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var booking = await eventProcessor.ProcessAsync(createBookingEvent);
        var updateBookingAddressEvent = new UpdateBookingAddressEvent(
            booking.Id,
            new Address("fromUpdated", "toUpdated", "zipUpdated", "zipcodeUpdated"),
            new Address("streetUpdated", "cityUpdated", "zipUpdated", "zipcodeUpdated")
        );

        booking = await eventProcessor.ProcessAsync(updateBookingAddressEvent);

        var bookingHistory = await entityHistoryService.GetEntityHistoryAsync<Booking>(booking.Id);

        Assert.Equal(2, bookingHistory.Count);
        Assert.Equal(createBookingEvent.From, bookingHistory.First().From);
        Assert.Equal(updateBookingAddressEvent.From, bookingHistory.Last().From);
    }

    [Fact]
    public async Task Test4()
    {
        var createBookingEvent = new CreateBookingEvent(
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var booking = await eventProcessor.ProcessAsync(createBookingEvent);
        var updateBookingAddressEvent = new UpdateBookingAddressEvent(
            booking.Id,
            new Address("fromUpdated", "toUpdated", "zipUpdated", "zipcodeUpdated"),
            new Address("streetUpdated", "cityUpdated", "zipUpdated", "zipcodeUpdated")
        );

        booking = await eventProcessor.ProcessAsync(updateBookingAddressEvent);

        var fetchedBooking = await entityStore.GetEntityByIdAsync<Booking>(booking.Id);
        Assert.NotNull(fetchedBooking);
        Assert.Equal(createBookingEvent.EntityId, fetchedBooking.Id);

        Assert.Equal(updateBookingAddressEvent.From, fetchedBooking.From);
    }

    [Fact]
    public async Task Test5()
    {
        var createBookingEvent = new CreateBookingEvent(
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var createBookingEventHandler = new CreateBookingEventHandler(entityStore);
        eventProcessor.RegisterEventHandler(createBookingEventHandler);
        var booking = await eventProcessor.ProcessAsync(createBookingEvent);

        var fetchedBooking = await entityStore.GetEntityByIdAsync<Booking>(booking.Id);
        Assert.Equal("Booking", fetchedBooking.Name);
    }

    [Fact]
    public async Task Test6()
    {
        var createBookingEvent = new CreateBookingEvent(
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var booking = await eventProcessor.ProcessAsync(createBookingEvent);
        var updateBookingAddressEvent = new UpdateBookingAddressEvent(
            booking.Id,
            new Address("fromUpdated", "toUpdated", "zipUpdated", "zipcodeUpdated"),
            new Address("streetUpdated", "cityUpdated", "zipUpdated", "zipcodeUpdated")
        );

        booking = await eventProcessor.ProcessAsync(updateBookingAddressEvent);

        await replayService.ReplayAllEventsAsync();

        var fetchedBooking = await entityStore.GetEntityByIdAsync<Booking>(booking.Id);
        Assert.Equal(updateBookingAddressEvent.From, fetchedBooking.From);
    }

    [Fact]
    public async Task Test7()
    {
        var createBookingEvent = new CreateBookingEvent(
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var booking = await eventProcessor.ProcessAsync(createBookingEvent);
        var updateBookingAddressEvent = new UpdateBookingAddressEvent(
            booking.Id,
            new Address("fromUpdated", "toUpdated", "zipUpdated", "zipcodeUpdated"),
            new Address("streetUpdated", "cityUpdated", "zipUpdated", "zipcodeUpdated")
        );

        booking = await eventProcessor.ProcessAsync(updateBookingAddressEvent);

        await replayService.ReplayEventAsync(createBookingEvent);

        var fetchedBooking = await entityStore.GetEntityByIdAsync<Booking>(booking.Id);
        Assert.Equal(createBookingEvent.From, fetchedBooking.From);
    }

    [Fact]
    public async Task Test8()
    {
        var createBookingEvent = new CreateBookingEvent(
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var booking = await eventProcessor.ProcessAsync(createBookingEvent);

        var addCustomerToBookingEvent = new AddCustomerToBookingEvent(booking.Id, "CustomerName");
        await eventProcessor.ProcessAsync(addCustomerToBookingEvent);

        var returnedBooking = await entityStore.GetEntityByIdAsync<Booking>(booking.Id);
        Assert.Equal("CustomerName", returnedBooking.CustomerName);
    }

    [Fact]
    public async Task Test9()
    {
        var createBookingEvent = new CreateBookingEvent(
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var booking = await eventProcessor.ProcessAsync(createBookingEvent);

        var addCustomerToBookingEvent = new AddCustomerToBookingEvent(booking.Id, "CustomerName");

        await eventProcessor.ProcessAsync(addCustomerToBookingEvent);

        var events = await eventStore.GetEventsAsync();
        Assert.Null(((AddCustomerToBookingEvent)events.Last()).CustomerName);
    }

    [Fact]
    public async Task Test10()
    {
        var createBookingEvent = new CreateBookingEvent(
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        await eventProcessor.ProcessAsync(createBookingEvent);

        var mongoDbEntityStore = new MongoDbEntityStore(mongoDbService);
        var booking = new Booking();
        booking.CustomerName = "Test10";
        await mongoDbEntityStore.SaveEntityAsync(booking);
        var bookingReturned = await mongoDbEntityStore.GetEntityByFilterAsync<Booking>(b =>
            b.CustomerName == "Test10"
        );
        Assert.NotNull(bookingReturned);
        Assert.Equal(booking.CustomerName, bookingReturned.CustomerName);
    }

    [Fact]
    public async Task Test11()
    {
        var createBookingEvent = new CreateBookingEvent(
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        var booking = await eventProcessor.ProcessAsync(createBookingEvent);

        var events = await eventStore.GetEventsAsync();
        Assert.Equal(1, events.Count);

        Assert.Equal(createBookingEvent.Id, events.First().Id);
        Assert.Equal(createBookingEvent.Timestamp, events.First().Timestamp);
    }

    [Fact]
    public async Task Test12()
    {
        var createBookingEvent = new CreateBookingEvent(
            new Address("from", "to", "zip", "zipcode"),
            new Address("street", "city", "zip", "zipcode")
        );

        await eventProcessor.ProcessAsync(createBookingEvent);

        var booking = new Booking();
        booking.CustomerName = "Test10";
        booking.Test = new Address("from", "to", "zip", "zipcode");
        await mongoDbEntityStore.SaveEntityAsync(booking);

        var address = await mongoDbEntityStore.GetProjectionByFilterAsync<Booking, Address>(
            b => b.CustomerName == "Test10",
            b => b.Test
        );
        Assert.NotNull(address);
        Assert.Equal(booking.Test, address);
    }

    public void Dispose()
    {
        using var cursor = mongoDbService
            .database.ListCollectionNamesAsync()
            .GetAwaiter()
            .GetResult();
        var collections = cursor.ToList();

        foreach (var collectionName in collections)
        {
            mongoDbService.database.DropCollectionAsync(collectionName).GetAwaiter().GetResult();
        }
    }
}
