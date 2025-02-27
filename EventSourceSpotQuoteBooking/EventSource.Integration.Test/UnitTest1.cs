using EventSource.Core.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EventSource.Core.Test;

public class UnitTest1 //: IDisposable
{
    // private readonly MongoEventStore mongoEventStore;
    // private readonly IMongoCollection<Event> eventCollection;
    // private readonly IEventProcessor eventProcessor;
    //
    // public UnitTest1()
    // {
    //     var mongoDbOptions = new MongoDbOptions
    //     {
    //         EventStore = new EventStoreOptions
    //         {
    //             ConnectionString = "mongodb://localhost:27017",
    //             DatabaseName = "EventSource"
    //         }
    //     };
    //
    //     var mongoDbIOptions = Options.Create(mongoDbOptions);
    //     var mongoDbService = new MongoDbService(mongoDbIOptions);
    //
    //     mongoEventStore = new MongoEventStore(mongoDbService);
    //
    //     eventCollection = mongoDbService.Database.GetCollection<Event>("events");
    //     eventCollection.DeleteMany(Builders<Event>.Filter.Empty);
    //
    //     var projectionHandler = new ProjectionHandler(mongoDbService);
    //     eventProcessor = new EventProcessor(mongoEventStore, projectionHandler);
    // }
    //
    // [Fact]
    // public async Task Test1()
    // {
    //     var testEvent = new TestEvent("from", "to", new Address("street", "city", "zip", "zipcode"));
    //     await mongoEventStore.SaveEventAsync(testEvent);
    //     var events = await mongoEventStore.GetEventsAsync();
    //     Assert.Single(events);
    //     Assert.Equal(testEvent.Id, events.First().Id);
    // }
    //
    // public void Dispose()
    // {
    //     eventCollection.DeleteMany(Builders<Event>.Filter.Empty);
    //}
}
