using EventSourcingFramework.Core.Models;
using EventSourcingFramework.Core.Models.Entity;

namespace EventSourcingFramework.Test.Utilities;

public class TestEntity : Entity
{
    public string Name { get; set; }
    public List<TestEntity> Children { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();

    public TestEntity()
    {
        SchemaVersion = 3;
    }
}
