namespace EventSource.Core.Test;

public class Booking : Entity
{
    public string CustomerName { get; set; }
    public Address From { get; private set; }
    public Address to { get; private set; }
    public string? Name { get; set; }

    public override void Apply(Event e)
    {
        Apply((dynamic)e);
    }

    private void Apply(CreateBookingEvent e)
    {
        From = e.From;
        to = e.To;
    }

    private void Apply(UpdateBookingAddressEvent e)
    {
        From = e.From;
        to = e.To;
    }

    private void Apply(AddCustomerToBookingEvent e) => CustomerName = e.CustomerName;
}
