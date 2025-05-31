using EventSourcingFramework.Core.Models.Entity;

namespace Example;

public class Order : Entity
{
    public Guid CustomerId { get; set; }
} 