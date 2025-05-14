using EventSourcingFramework.Core.Models.Entity;

namespace EventSourcingFramework.Test.Utilities.Models;

public class TestEntity2 : Entity
{
    public string FirstName { get; set; }
    public string SurName { get; set; }
    public int Age { get; set; }

    public TestEntity2()
    {
        SchemaVersion = 2;
    }
}
