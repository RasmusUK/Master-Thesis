using MongoDB.Bson.Serialization.Attributes;

namespace EventSourcingFramework.Infrastructure.Shared.Models;

public record MongoPersonalData
{
    [BsonId]
    public Guid EventId { get; set; }
    public Dictionary<string, object?> Data { get; set; } = new();
}
