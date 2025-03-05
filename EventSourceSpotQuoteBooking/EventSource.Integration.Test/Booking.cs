namespace EventSource.Core.Test;

public class Booking : Entity
{
    private Address from;
    private Address to;

    public override void Apply(Event e)
    {
        Apply((dynamic)e);
    }

    public Address GetFrom() => from;

    private void Apply(CreateBookingEvent e)
    {
        from = e.From;
        to = e.To;
    }

    private void Apply(object e) =>
        throw new InvalidOperationException($"Could not apply event {e.GetType().Name}");
}
