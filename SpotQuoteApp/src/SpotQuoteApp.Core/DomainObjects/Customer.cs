using EventSourcingFramework.Core.Attributes;
using EventSourcingFramework.Core.Models.Entity;
using SpotQuoteApp.Core.ValueObjects;

namespace SpotQuoteApp.Core.DomainObjects;

public class Customer : Entity
{
    public Customer(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
}
