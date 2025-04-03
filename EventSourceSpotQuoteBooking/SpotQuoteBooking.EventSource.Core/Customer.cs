using EventSource.Core;

namespace SpotQuoteBooking.EventSource.Core;

public class Customer : Entity
{
    public string Name { get; set; }

    public Customer(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }
}
