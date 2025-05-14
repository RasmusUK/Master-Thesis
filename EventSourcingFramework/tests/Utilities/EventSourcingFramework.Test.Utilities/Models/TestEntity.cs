using EventSourcingFramework.Core.Models.Entity;

namespace EventSourcingFramework.Test.Utilities.Models;

public class TestEntity : Entity
{
    public TestEntity()
    {
        SchemaVersion = 3;
    }

    public string Name { get; set; }
    public List<TestEntity> Children { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}