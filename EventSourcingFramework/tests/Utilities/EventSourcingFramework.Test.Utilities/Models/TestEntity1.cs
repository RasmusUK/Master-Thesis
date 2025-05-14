using EventSourcingFramework.Core.Models.Entity;

namespace EventSourcingFramework.Test.Utilities.Models;

public class TestEntity1 : Entity
{
    public TestEntity1()
    {
        SchemaVersion = 1;
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }
}