using EventSourcingFramework.Core.Attributes;
using EventSourcingFramework.Core.Models.Entity;
namespace FrameworkEvaluation;

public class Customerv1:Entity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public new int SchmeaVersion { get; set; } = 1;
    public Customerv1(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}

public class Customer : Entity
{
    [PersonalData]
    public string Name { get; set; }
    public new int SchmeaVersion { get; set; } = 2;

    public Customer(string name)
    {
        Name = name;
        
    }
}
