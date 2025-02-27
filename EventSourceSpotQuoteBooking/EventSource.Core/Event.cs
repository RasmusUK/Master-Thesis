using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventSource.Core;

public abstract class Event
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; private set; } = Guid.NewGuid();
    [BsonRepresentation(BsonType.String)]
    public Guid AggregateId { get; protected set; }
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
    public string Type { get; protected set; }

    protected Event()
    {
        Type = GetType().ToString();
    }
}