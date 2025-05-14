using MongoDB.Bson;

namespace EventSourcing.Framework.Infrastructure.Shared.Models;

public record MongoApiResponse
{
    public string Key { get; set; }
    public BsonDocument Response { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}