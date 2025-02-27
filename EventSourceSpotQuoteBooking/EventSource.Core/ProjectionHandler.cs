using MongoDB.Driver;

namespace EventSource.Core;

public class ProjectionHandler : IProjectionHandler
{
    private readonly IMongoCollection<AggregateRoot> collection;
    
    public ProjectionHandler(IMongoDbService mongoDbService)
    {
        collection = mongoDbService.Database.GetCollection<AggregateRoot>("aggregateRoots");
    }
    public Task HandleAsync(Event e)
    {
        switch (e)
        {
            case BookingCreatedEvent bookingCreatedEvent:
                return HandleOrderCreatedEvent(bookingCreatedEvent);
        }
        return Task.CompletedTask;
    }

    private Task HandleOrderCreatedEvent(BookingCreatedEvent e)
    {
        var aggregateRoot = new Booking(e.From, e.To);
        return collection.InsertOneAsync(aggregateRoot);
    }
}