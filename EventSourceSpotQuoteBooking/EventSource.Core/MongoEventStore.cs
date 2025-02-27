using MongoDB.Driver;

namespace EventSource.Core;

public class MongoEventStore : IEventStore
{
    private readonly IMongoCollection<Event> eventCollection;

    public MongoEventStore(IMongoDbService mongoDbService)
    {
        eventCollection = mongoDbService.Database.GetCollection<Event>("events");
    }

    public Task SaveEventAsync(Event e)
        => eventCollection.InsertOneAsync(e);

    public Task<List<Event>> GetEventsAsync()
        => eventCollection.Find(_ => true).ToListAsync();

}