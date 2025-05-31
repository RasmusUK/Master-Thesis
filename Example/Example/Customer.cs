using EventSourcingFramework.Core.Models.Entity;

namespace Example;

public class CustomerV1 : Entity
{
    public int SchemaVersion { get; set; } = 1; 
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class Customer : Entity
{
    public int SchemaVersion { get; set; } = 2;
    public string Name { get; set; }
}