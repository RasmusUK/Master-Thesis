namespace EventSource.Core.Test;

public class UpdateBookingAddressEvent : Event
{
    public Address From { get; init; }
    public Address To { get; init; }

    public UpdateBookingAddressEvent(Guid entityId, Address to, Address from)
        : base(entityId)
    {
        To = to;
        From = from;
    }
}
