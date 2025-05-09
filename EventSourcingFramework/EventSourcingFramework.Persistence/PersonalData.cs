using MongoDB.Bson.Serialization.Attributes;

namespace EventSourcingFramework.Persistence;

public record PersonalData
{
    [BsonId]
    public Guid EventId { get; set; }
    public Dictionary<string, object?> Data { get; set; } = new();
}
