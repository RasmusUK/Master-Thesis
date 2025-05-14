using EventSourcingFramework.Core.Models.Events;

namespace EventSourcingFramework.Test.Utilities.Models;

public record TestEvent : IEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; }
    public Guid EntityId { get; }
    public Guid? TransactionId { get; }
    public long EventNumber { get; }
    public bool Compensation { get; }
    public string Typename { get; }
}