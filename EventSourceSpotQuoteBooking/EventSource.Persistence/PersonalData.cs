using MongoDB.Bson.Serialization.Attributes;

namespace EventSource.Persistence;

public record PersonalData
{
    [BsonId]
    public Guid EventId { get; set; }
    public Dictionary<string, object?> Data { get; set; } = new();
}
