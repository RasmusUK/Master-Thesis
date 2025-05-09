namespace EventSourcingFramework.Core.Models.Events;

public interface IEvent
{
    Guid Id { get; }
    DateTime Timestamp { get; }
    Guid EntityId { get; }
    Guid? TransactionId { get; }
    long EventNumber { get; }
    bool Compensation { get; }
    string Typename { get; }
}
