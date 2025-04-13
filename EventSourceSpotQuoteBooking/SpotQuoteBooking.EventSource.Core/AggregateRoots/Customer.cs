using EventSource.Core;
using SpotQuoteBooking.EventSource.Core.ValueObjects;

namespace SpotQuoteBooking.EventSource.Core.AggregateRoots;

public class Customer : Entity
{
    public Customer(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
}
