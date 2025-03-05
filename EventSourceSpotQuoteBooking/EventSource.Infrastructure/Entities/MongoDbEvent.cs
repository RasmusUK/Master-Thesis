using EventSource.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventSource.Persistence.Entities;

public class MongoDbEvent : MongoDbBase<Event>
{
    [BsonElement("aggregateId")]
    [BsonRepresentation(BsonType.String)]
    public Guid? AggregateId { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    public MongoDbEvent(Event domainEvent)
        : base(domainEvent.Id, domainEvent)
    {
        AggregateId = domainEvent.AggregateId;
        Timestamp = domainEvent.Timestamp;
    }
}
