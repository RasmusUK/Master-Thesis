namespace EventSource.Core.Unit.Test;

public class TestEvent : Event
{
    public TestEvent(Guid entityId)
        : base(entityId) { }
}
