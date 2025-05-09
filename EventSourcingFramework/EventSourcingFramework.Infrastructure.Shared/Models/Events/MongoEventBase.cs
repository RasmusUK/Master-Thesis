using EventSourcingFramework.Core.Models.Events;
using MongoDB.Bson.Serialization.Attributes;

namespace EventSourcing.Framework.Infrastructure.Shared.Models.Events;

public abstract record MongoEventBase(Guid EntityId) : IEvent
{
    [BsonId]
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid? TransactionId { get; init; }
    public long EventNumber { get; set; }
    public bool Compensation { get; init; }

    [BsonIgnore]
    public string Typename =>
        GetType().IsGenericType
            ? $"{GetType().Name.Replace("Mongo","").Split('`')[0]}<{string.Join(", ", GetType().GetGenericArguments().Select(t => t.Name))}>"
            : GetType().Name;
}
