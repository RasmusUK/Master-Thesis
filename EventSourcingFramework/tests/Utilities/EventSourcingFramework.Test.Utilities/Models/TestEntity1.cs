using EventSourcingFramework.Core.Models.Entity;

namespace EventSourcingFramework.Test.Utilities.Models;

public class TestEntity1 : Entity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public TestEntity1()
    {
        SchemaVersion = 1;
    }
}
