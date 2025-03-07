namespace EventSource.Core.Test;

public record UpdateBookingAddressEvent : Event
{
    public Address From { get; init; }
    public Address To { get; init; }

    public UpdateBookingAddressEvent() { }

    public UpdateBookingAddressEvent(Guid entityId, Address to, Address from)
        : base(entityId)
    {
        To = to;
        From = from;
    }
}
