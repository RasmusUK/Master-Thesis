using EventSource.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventSource.Persistence.Entities;

public class MongoDbEvent : MongoDbBase<Event>
{
    [BsonElement(nameof(EntityId))]
    [BsonRepresentation(BsonType.String)]
    public Guid EntityId { get; set; }

    [BsonElement(nameof(EventNumber))]
    public long EventNumber { get; set; }

    [BsonElement(nameof(Timestamp))]
    public DateTime Timestamp { get; set; }

    public MongoDbEvent(Event domainEvent)
        : base(domainEvent.Id, domainEvent)
    {
        if (domainEvent.EntityId == Guid.Empty)
            throw new ArgumentException($"Entity id cannot be empty", nameof(domainEvent.EntityId));
        EntityId = domainEvent.EntityId!.Value;
        Timestamp = domainEvent.Timestamp;
        EventNumber = domainEvent.EventNumber;
    }
}
