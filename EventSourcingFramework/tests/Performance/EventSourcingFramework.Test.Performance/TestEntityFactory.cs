namespace EventSourcingFramework.Test.Performance;

public static class TestEntityFactory
{
    public static TestEntity CreateEntity()
    {
        return new TestEntity
        {
            Id = Guid.NewGuid(),
            ConcurrencyVersion = 1,
            SchemaVersion = 1,
            Name = string.Empty,
            Name1 = "Name",
            Name2 = "NameName",
            Name3 = "NameNameName",
            Name4 = "NameNameNameName",
            Name5 = "NameNameNameNameName",
            Nr1 = 1,
            Nr2 = 10,
            Nr3 = 100,
            Nr4 = 1000,
            Nr5 = 10000,
            Nr6 = 100000,
            Nr7 = 1000000,
            Nr8 = 10000000,
            Nr9 = 100000000,
            Nr10 = 1000000000
        };
    }
}