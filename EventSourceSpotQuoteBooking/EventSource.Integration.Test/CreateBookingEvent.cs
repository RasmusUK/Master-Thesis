namespace EventSource.Core.Test;

public class CreateBookingEvent : Event
{
    public Address From { get; init; }
    public Address To { get; init; }

    public CreateBookingEvent(Guid aggregateId, Address from, Address to)
        : base(aggregateId)
    {
        From = from;
        To = to;
    }
}
