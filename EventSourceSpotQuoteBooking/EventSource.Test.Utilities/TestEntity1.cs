using EventSource.Persistence;

namespace EventSource.Test.Utilities;

public class TestEntity1 : Entity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public TestEntity1()
    {
        SchemaVersion = 1;
    }
}
