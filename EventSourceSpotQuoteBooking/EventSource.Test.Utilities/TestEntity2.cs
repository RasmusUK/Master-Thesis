using EventSource.Persistence;

namespace EventSource.Test.Utilities;

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
