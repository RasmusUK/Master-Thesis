namespace EventSourcingFramework.Core.Unit.Test;

public record TestEvent : Event
{
    public TestEvent(Guid entityId)
        : base(entityId) { }
}
