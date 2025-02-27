using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventSource.Core;

public abstract class AggregateRoot
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; private set; } = Guid.NewGuid();
}