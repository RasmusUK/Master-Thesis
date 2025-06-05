using MongoDB.Bson;

namespace EventSourcingFramework.Infrastructure.Shared.Models;

public record MongoApiResponse
{
    public string Key { get; set; }
    public BsonDocument Response { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long EventNumber { get; set; } = 0;
}