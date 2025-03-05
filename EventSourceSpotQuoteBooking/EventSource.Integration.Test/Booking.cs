namespace EventSource.Core.Test;

public class Booking : Entity
{
    public string CustomerName { get; set; }
    private Address from;
    private Address to;
    public string? Name { get; set; }

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

    private void Apply(UpdateBookingAddressEvent e)
    {
        from = e.From;
        to = e.To;
    }

    private void Apply(AddCustomerToBookingEvent e) => CustomerName = e.CustomerName;

    private void Apply(object e) =>
        throw new InvalidOperationException($"Could not apply event {e.GetType().Name}");
}
