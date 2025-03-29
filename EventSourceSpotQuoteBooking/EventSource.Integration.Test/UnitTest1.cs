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
    private readonly IRepository<Quote> quoteRepository;

    public UnitTest1(
        IEventProcessor eventProcessor,
        IEntityStore entityStore,
        IEventStore eventStore,
        IEntityHistoryService entityHistoryService,
        IReplayService replayService,
        IMongoDbService mongoDbService,
        IEntityStore mongoDbEntityStore,
        IRepository<Quote> quoteRepository
    )
    {
        this.eventProcessor = eventProcessor;
        this.entityStore = entityStore;
        this.eventStore = eventStore;
        this.entityHistoryService = entityHistoryService;
        this.replayService = replayService;
        this.mongoDbService = mongoDbService;
        this.mongoDbEntityStore = mongoDbEntityStore;
        this.quoteRepository = quoteRepository;
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
        await mongoDbEntityStore.UpsertEntityAsync(booking);
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
        await mongoDbEntityStore.UpsertEntityAsync(booking);

        var address = await mongoDbEntityStore.GetProjectionByFilterAsync<Booking, Address>(
            b => b.CustomerName == "Test10",
            b => b.Test
        );
        Assert.NotNull(address);
        Assert.Equal(booking.Test, address);
    }

    [Fact]
    public async Task RepoCreateReadById()
    {
        var quote = new Quote(100.0, "DKK", "QuoteTest");

        var quoteId = await quoteRepository.CreateAsync(quote);
        var quoteReturned = await quoteRepository.ReadByIdAsync(quoteId);
        Assert.NotNull(quoteReturned);
        Assert.Equal(quote.Id, quoteReturned.Id);
        Assert.Equal(quote.Price, quoteReturned.Price);
        Assert.Equal(quote.Currency, quoteReturned.Currency);
        Assert.Equal(quote.Name, quoteReturned.Name);
    }

    [Fact]
    public async Task RepoReadAll()
    {
        var quote1 = new Quote(100.0, "DKK", "QuoteTest1");
        var quote2 = new Quote(100.0, "DKK", "QuoteTest2");

        await quoteRepository.CreateAsync(quote1);
        await quoteRepository.CreateAsync(quote2);
        var quotes = await quoteRepository.ReadAllAsync();
        Assert.Equal(2, quotes.Count);
    }

    [Fact]
    public async Task RepoReadByFilter()
    {
        var quote1 = new Quote(100.0, "DKK", "QuoteTest1");
        var quote2 = new Quote(100.0, "DKK", "QuoteTest2");

        await quoteRepository.CreateAsync(quote1);
        await quoteRepository.CreateAsync(quote2);
        var quote = await quoteRepository.ReadByFilterAsync(q => q.Name == "QuoteTest1");
        Assert.NotNull(quote);
        Assert.Equal(quote1.Id, quote.Id);
    }

    [Fact]
    public async Task RepoReadAllByFilter()
    {
        var quote1 = new Quote(100.0, "DKK", "QuoteTest1");
        var quote2 = new Quote(100.0, "DKK", "QuoteTest2");
        var quote3 = new Quote(200.0, "DKK", "QuoteTest3");

        await quoteRepository.CreateAsync(quote1);
        await quoteRepository.CreateAsync(quote2);
        await quoteRepository.CreateAsync(quote3);
        var quotes = await quoteRepository.ReadAllByFilterAsync(q => q.Price == 100);
        Assert.Equal(2, quotes.Count);
    }

    [Fact]
    public async Task RepoReadProjectionById()
    {
        var quote1 = new Quote(100.0, "DKK", "QuoteTest1");
        var quote2 = new Quote(100.0, "DKK", "QuoteTest2");
        var quote3 = new Quote(200.0, "DKK", "QuoteTest3");

        await quoteRepository.CreateAsync(quote1);
        await quoteRepository.CreateAsync(quote2);
        await quoteRepository.CreateAsync(quote3);
        var price = await quoteRepository.ReadProjectionByIdAsync(quote1.Id, q => q.Price);
        Assert.Equal(quote1.Price, price);
    }

    [Fact]
    public async Task RepoReadProjectionByFilter()
    {
        var quote1 = new Quote(100.0, "DKK", "QuoteTest1");
        var quote2 = new Quote(100.0, "DKK", "QuoteTest2");
        var quote3 = new Quote(200.0, "DKK", "QuoteTest3");

        await quoteRepository.CreateAsync(quote1);
        await quoteRepository.CreateAsync(quote2);
        await quoteRepository.CreateAsync(quote3);
        var price = await quoteRepository.ReadProjectionByFilterAsync(
            q => q.Id == quote1.Id,
            q => q.Price
        );
        Assert.Equal(quote1.Price, price);
    }

    [Fact]
    public async Task RepoReadAllProjections()
    {
        var quote1 = new Quote(100.0, "DKK", "QuoteTest1");
        var quote2 = new Quote(100.0, "DKK", "QuoteTest2");
        var quote3 = new Quote(200.0, "DKK", "QuoteTest3");

        await quoteRepository.CreateAsync(quote1);
        await quoteRepository.CreateAsync(quote2);
        await quoteRepository.CreateAsync(quote3);
        var prices = await quoteRepository.ReadAllProjectionsAsync(q => q.Price);
        Assert.Equal(3, prices.Count);
    }

    [Fact]
    public async Task RepoReadAllProjectionsByFilter()
    {
        var quote1 = new Quote(100.0, "DKK", "QuoteTest1");
        var quote2 = new Quote(100.0, "DKK", "QuoteTest2");
        var quote3 = new Quote(200.0, "DKK", "QuoteTest3");

        await quoteRepository.CreateAsync(quote1);
        await quoteRepository.CreateAsync(quote2);
        await quoteRepository.CreateAsync(quote3);
        var prices = await quoteRepository.ReadAllProjectionsByFilterAsync(
            q => q.Price,
            q => q.Price == 100
        );
        Assert.Equal(2, prices.Count);
    }

    [Fact]
    public async Task RepoUpdate()
    {
        var quote = new Quote(100.0, "DKK", "QuoteTest1");

        var id = await quoteRepository.CreateAsync(quote);

        quote = await quoteRepository.ReadByIdAsync(id);
        Assert.Equal(100, quote.Price);

        quote.Price = 200;
        await quoteRepository.UpdateAsync(quote);

        quote = await quoteRepository.ReadByIdAsync(id);
        Assert.Equal(200, quote.Price);
    }

    [Fact]
    public async Task RepoDelete()
    {
        var quote = new Quote(100.0, "DKK", "QuoteTest1");

        var id = await quoteRepository.CreateAsync(quote);

        quote = await quoteRepository.ReadByIdAsync(id);
        Assert.NotNull(quote);

        await quoteRepository.DeleteAsync(quote);
        quote = await quoteRepository.ReadByIdAsync(id);
        Assert.Null(quote);
    }

    [Fact]
    public async Task RepoCreateCreatesEvent()
    {
        var quote = new Quote(100.0, "DKK", "QuoteTest1");

        var id = await quoteRepository.CreateAsync(quote);

        var events = await eventStore.GetEventsByEntityIdAsync(id);
        Assert.Single(events);
        var e = events.First();
        Assert.Equal(e.EntityId, id);
    }

    [Fact]
    public async Task RepoCreateUpdateEvent()
    {
        var quote = new Quote(100.0, "DKK", "QuoteTest1");

        var id = await quoteRepository.CreateAsync(quote);

        quote.Price = 200;
        await quoteRepository.UpdateAsync(quote);

        var events = await eventStore.GetEventsByEntityIdAsync(id);
        Assert.Equal(2, events.Count);
    }

    [Fact]
    public async Task RepoCreateDeleteEvent()
    {
        var quote = new Quote(100.0, "DKK", "QuoteTest1");

        var id = await quoteRepository.CreateAsync(quote);

        await quoteRepository.DeleteAsync(quote);

        var events = await eventStore.GetEventsByEntityIdAsync(id);
        Assert.Equal(2, events.Count);
    }

    [Fact]
    public async Task RepoReplayEvent()
    {
        var quote = new Quote(100.0, "DKK", "QuoteTest1");

        var id = await quoteRepository.CreateAsync(quote);

        quote.Price = 200;
        await quoteRepository.UpdateAsync(quote);

        quote.Price = 300;
        await quoteRepository.UpdateAsync(quote);

        var events = await eventStore.GetEventsByEntityIdAsync(id);
        Assert.Equal(3, events.Count);

        await replayService.ReplayEventAsync(events.ToList()[1]);
        var quoteReturned = await quoteRepository.ReadByIdAsync(id);
        Assert.Equal(200, quoteReturned.Price);
    }

    [Fact]
    public async Task RepoReplayEvents()
    {
        var quote = new Quote(100.0, "DKK", "QuoteTest1");

        var id = await quoteRepository.CreateAsync(quote);

        quote.Price = 200;
        await quoteRepository.UpdateAsync(quote);

        quote.Price = 300;
        await quoteRepository.UpdateAsync(quote);

        await quoteRepository.DeleteAsync(quote);

        var events = (await eventStore.GetEventsByEntityIdAsync(id)).ToList();

        await replayService.ReplayEventsAsync(events.Take(3).ToList());
        var quoteReturned = await quoteRepository.ReadByIdAsync(id);
        Assert.Equal(300, quoteReturned.Price);

        await replayService.ReplayEventAsync(events.Last());

        var quoteReturned2 = await quoteRepository.ReadByIdAsync(id);
        Assert.Null(quoteReturned2);
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
