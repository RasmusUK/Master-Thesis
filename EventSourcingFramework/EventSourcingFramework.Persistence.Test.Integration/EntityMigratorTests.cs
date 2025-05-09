using EventSourcingFramework.Persistence.Interfaces;
using EventSourcingFramework.Test.Utilities;

namespace EventSourcingFramework.Persistence.Test.Integration;

public class EntityMigratorTests
{
    private readonly IEntityMigrator migrator = new EntityMigrator();

    [Fact]
    public void Register_Migration_From_v1_To_v2_Works()
    {
        // Arrange
        migrator.Register<TestEntity1, TestEntity2>(
            1,
            e => new TestEntity2
            {
                Id = e.Id,
                FirstName = e.FirstName,
                SurName = e.LastName,
                Age = 0,
            }
        );

        var v1 = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
        };

        // Act
        var migrated = migrator.Migrate(v1, typeof(TestEntity1), typeof(TestEntity2), 1);

        // Assert
        var result = Assert.IsType<TestEntity2>(migrated);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.SurName);
        Assert.Equal(2, result.SchemaVersion);
    }

    [Fact]
    public void Register_Migration_From_v2_To_v3_Works()
    {
        // Arrange
        migrator.Register<TestEntity2, TestEntity>(
            2,
            e => new TestEntity { Id = e.Id, Name = $"{e.FirstName} - {e.SurName}" }
        );

        var v2 = new TestEntity2
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            SurName = "Smith",
            Age = 30,
        };

        // Act
        var migrated = migrator.Migrate(v2, typeof(TestEntity2), typeof(TestEntity), 2);

        // Assert
        var result = Assert.IsType<TestEntity>(migrated);
        Assert.Equal("Jane - Smith", result.Name);
        Assert.Equal(3, result.SchemaVersion);
    }

    [Fact]
    public void Migrate_SameType_ReturnsOriginal()
    {
        // Arrange
        var current = new TestEntity { Id = Guid.NewGuid(), Name = "NoChange" };

        // Act
        var result = migrator.Migrate(current, typeof(TestEntity), typeof(TestEntity), 3);

        // Assert
        Assert.Same(current, result);
    }

    [Fact]
    public void Migrate_Unregistered_Throws()
    {
        // Arrange
        var entity = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "A",
            LastName = "B",
        };

        // Act & Assert
        var ex = Assert.Throws<Exception>(
            () => migrator.Migrate(entity, typeof(TestEntity1), typeof(TestEntity2), 1)
        );
        Assert.Contains("No migration registered", ex.Message);
    }

    [Fact]
    public void Chain_Migration_v1_To_v3_Through_v2()
    {
        // Arrange
        migrator.Register<TestEntity1, TestEntity2>(
            1,
            e => new TestEntity2
            {
                Id = e.Id,
                FirstName = e.FirstName,
                SurName = e.LastName,
                Age = 0,
            }
        );

        migrator.Register<TestEntity2, TestEntity>(
            2,
            e => new TestEntity { Id = e.Id, Name = $"{e.FirstName} - {e.SurName}" }
        );

        var v1 = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Wonder",
        };

        // Act
        var v2 = (TestEntity2)migrator.Migrate(v1, typeof(TestEntity1), typeof(TestEntity2), 1);
        var v3 = (TestEntity)migrator.Migrate(v2, typeof(TestEntity2), typeof(TestEntity), 2);

        // Assert
        Assert.Equal("Alice - Wonder", v3.Name);
        Assert.Equal(3, v3.SchemaVersion);
    }
}
