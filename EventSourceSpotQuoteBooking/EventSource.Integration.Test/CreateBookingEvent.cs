namespace EventSource.Core.Test;

public record CreateBookingEvent : Event
{
    public Address From { get; init; }
    public Address To { get; init; }

    public CreateBookingEvent(Guid entityId, Address from, Address to)
        : base(entityId)
    {
        From = from;
        To = to;
    }
}
