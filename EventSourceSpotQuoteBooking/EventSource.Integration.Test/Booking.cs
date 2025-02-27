namespace EventSource.Core.Test;

public class Booking : AggregateRoot
{
    private readonly Address from;
    private readonly Address to;

    public Booking(Address from, Address to)
    {
        this.from = from;
        this.to = to;
    }
}
