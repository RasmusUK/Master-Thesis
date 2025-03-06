using EventSource.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventSource.Persistence.Entities;

public class MongoDbEvent : MongoDbBase<Event>
{
    [BsonElement(nameof(EventId))]
    [BsonRepresentation(BsonType.String)]
    public Guid EventId { get; set; }

    [BsonElement(nameof(Timestamp))]
    public DateTime Timestamp { get; set; }

    public MongoDbEvent(Event domainEvent)
        : base(domainEvent.Id, domainEvent)
    {
        EventId = domainEvent.EntityId;
        Timestamp = domainEvent.Timestamp;
    }
}
