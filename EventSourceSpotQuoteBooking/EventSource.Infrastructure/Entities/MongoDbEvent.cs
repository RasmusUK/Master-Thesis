using EventSource.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace EventSource.Persistence.Entities;

public class MongoDbEvent : MongoDbEntity<Event>
{
    [BsonElement("aggregateId")]
    [BsonRepresentation(BsonType.String)]
    public Guid AggregateId { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    public MongoDbEvent(Event domainEvent)
        : base(domainEvent.Id, domainEvent)
    {
        AggregateId = domainEvent.AggregateId;
        Timestamp = domainEvent.Timestamp;
    }
}
