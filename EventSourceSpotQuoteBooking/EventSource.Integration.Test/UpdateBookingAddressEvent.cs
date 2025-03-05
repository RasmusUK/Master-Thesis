namespace EventSource.Core.Test;

public class UpdateBookingAddressEvent : Event
{
    public Address From { get; init; }
    public Address To { get; init; }

    public UpdateBookingAddressEvent(Guid aggregateId, Address to, Address from)
        : base(aggregateId)
    {
        To = to;
        From = from;
    }
}
