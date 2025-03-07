namespace EventSource.Core.Test;

public class Booking : Entity
{
    public string CustomerName { get; set; }
    public Address From { get; private set; }
    public Address To { get; private set; }
    public Address Test { get; set; }
    public string? Name { get; set; }

    public override void Apply(Event e)
    {
        Apply((dynamic)e);
    }

    private void Apply(CreateBookingEvent e)
    {
        From = e.From;
        To = e.To;
    }

    private void Apply(UpdateBookingAddressEvent e)
    {
        From = e.From;
        To = e.To;
    }

    private void Apply(AddCustomerToBookingEvent e) => CustomerName = e.CustomerName;
}
