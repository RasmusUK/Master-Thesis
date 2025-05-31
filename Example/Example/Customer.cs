using EventSourcingFramework.Core.Attributes;
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
    [PersonalData]
    public string Name { get; set; }
}