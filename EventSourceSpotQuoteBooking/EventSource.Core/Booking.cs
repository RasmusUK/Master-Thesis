using EventSource.Core.Test;

namespace EventSource.Core;

public class Booking : AggregateRoot
{
    public Address From { get; private set; }
    public Address To { get; private set; }

    public Booking(Address from, Address to)
    {
        From = from;
        To = to;
    }
}