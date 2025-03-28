namespace EventSource.Core.Test;

public class Booking : Entity
{
    public string CustomerName { get; set; }
    public Address From { get; set; }
    public Address To { get; set; }
    public Address Test { get; set; }
    public string? Name { get; set; }

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
