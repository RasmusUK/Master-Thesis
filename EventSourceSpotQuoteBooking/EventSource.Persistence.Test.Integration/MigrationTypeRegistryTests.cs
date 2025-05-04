using EventSource.Persistence.Interfaces;
using EventSource.Test.Utilities;

namespace EventSource.Persistence.Test.Integration;

public class MigrationTypeRegistryTests
{
    private readonly IMigrationTypeRegistry registry = new MigrationTypeRegistry();

    [Fact]
    public void Register_And_GetVersionedType_ReturnsCorrectType()
    {
        // Arrange
        registry.Register<TestEntity>(1, typeof(TestEntity1));
        registry.Register<TestEntity>(2, typeof(TestEntity2));
        registry.Register<TestEntity>(3, typeof(TestEntity));

        // Act
        var type1 = registry.GetVersionedType(typeof(TestEntity), 1);
        var type2 = registry.GetVersionedType(typeof(TestEntity), 2);
        var type3 = registry.GetVersionedType(typeof(TestEntity), 3);

        // Assert
        Assert.Equal(typeof(TestEntity1), type1);
        Assert.Equal(typeof(TestEntity2), type2);
        Assert.Equal(typeof(TestEntity), type3);
    }

    [Fact]
    public void GetVersionedType_Unregistered_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        Assert.Throws<KeyNotFoundException>(
            () => registry.GetVersionedType(typeof(TestEntity), 99)
        );
    }

    [Fact]
    public void Register_OverwritesExistingVersion()
    {
        // Arrange
        registry.Register<TestEntity>(1, typeof(TestEntity1));

        // Act
        registry.Register<TestEntity>(1, typeof(TestEntity2));

        var resolved = registry.GetVersionedType(typeof(TestEntity), 1);

        // Assert
        Assert.Equal(typeof(TestEntity2), resolved);
    }
}
