using EventSource.Core;
using EventSource.Core.Events;

namespace EventSource.Test.Utilities;

public record UpdateEvent<T> : IUpdateEvent<T>
    where T : IEntity
{
    public T Entity { get; set; }
    public Guid Id { get; }
    public DateTime Timestamp { get; }
    public Guid EntityId { get; }
    public Guid? TransactionId { get; }
    public long EventNumber { get; }
    public bool Compensation { get; }
    public string Typename { get; }
}
