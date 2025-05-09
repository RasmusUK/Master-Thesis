using MongoDB.Bson;

namespace EventSourcingFramework.Test.Performance;

public static class TestEntityFactory
{
    public static TestEntity CreateEntityBySize(string size)
    {
        return size switch
        {
            "S1" => CreateNested(depth: 1, width: 1),
            "S2" => CreateNested(depth: 3, width: 4),
            "S3" => CreateNested(depth: 4, width: 6),
            "S4" => CreateNested(depth: 4, width: 10),
            "S5" => CreateNested(depth: 5, width: 8),
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null),
        };
    }

    private static TestEntity CreateNested(int depth = 5, int width = 5)
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
            Nr10 = 1000000000,
            Children = GenerateChildren(depth, width),
        };
    }

    private static List<TestEntity> GenerateChildren(int depth, int width)
    {
        if (depth <= 0)
            return new List<TestEntity>();

        return Enumerable
            .Range(0, width)
            .Select(i => new TestEntity
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
                Nr10 = 1000000000,
                Children = GenerateChildren(depth - 1, width),
            })
            .ToList();
    }

    public static double GetBsonSizeInMb<T>(T obj)
    {
        var bson = obj.ToBson();
        return bson.Length / (1024.0 * 1024.0);
    }

    public static int GetPropertyCountBySizeName(string size) => GetNodeCountBySizeName(size) * 20;

    public static int GetNodeCountBySizeName(string size)
    {
        var (depth, width) = size switch
        {
            "S1" => (1, 1),
            "S2" => (3, 4),
            "S3" => (4, 6),
            "S4" => (4, 10),
            "S5" => (5, 8),
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null),
        };

        if (width == 1)
            return depth;

        return (int)((Math.Pow(width, depth) - 1) / (width - 1));
    }
}
