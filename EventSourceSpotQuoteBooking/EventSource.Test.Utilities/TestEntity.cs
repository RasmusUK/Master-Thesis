using EventSource.Persistence;

namespace EventSource.Test.Utilities;

public class TestEntity : Entity
{
    public string Name { get; set; }

    public TestEntity()
    {
        SchemaVersion = 3;
    }
}
