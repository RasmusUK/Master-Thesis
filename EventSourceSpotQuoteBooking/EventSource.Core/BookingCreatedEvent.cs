using EventSource.Core.Test;

namespace EventSource.Core;

public class BookingCreatedEvent : Event
{
    public BookingCreatedEvent(Address from, Address to)
    {
        From = from;
        To = to;
    }

    public Address From { get; private set; }
    public Address To { get; private set; }
}