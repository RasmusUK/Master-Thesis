using EventSource.Core;
using SpotQuoteBooking.Shared;

namespace SpotQuoteBooking.EventSource.Core;

public class Customer : Entity
{
    public string Name { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();

    public Customer(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }
}
