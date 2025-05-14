using EventSourcingFramework.Application.Abstractions.Migrations;
using EventSourcingFramework.Infrastructure.Migrations.Services;
using EventSourcingFramework.Test.Utilities;
using EventSourcingFramework.Test.Utilities.Models;

namespace EventSourcingFramework.Infrastructure.Test.Integration.Migrations;

public class SchemaVersionRegistryTests
{
    private readonly ISchemaVersionRegistry registry = new SchemaVersionRegistry();

    [Fact]
    public void Register_Then_GetVersion_ReturnsCorrectVersion()
    {
        // Arrange
        registry.Register(typeof(TestEntity), 3);

        // Act
        var version = registry.GetVersion(typeof(TestEntity));

        // Assert
        Assert.Equal(3, version);
    }

    [Fact]
    public void GetVersion_UnregisteredType_ReturnsDefaultVersionOne()
    {
        // Act
        var version = registry.GetVersion(typeof(TestEntity1));

        // Assert
        Assert.Equal(1, version);
    }

    [Fact]
    public void GetVersion_Generic_ReturnsCorrectVersion()
    {
        // Arrange
        registry.Register(typeof(TestEntity2), 2);

        // Act
        var version = registry.GetVersion<TestEntity2>();

        // Assert
        Assert.Equal(2, version);
    }

    [Fact]
    public void Register_OverridesPreviousVersion()
    {
        // Arrange
        registry.Register(typeof(TestEntity), 2);
        registry.Register(typeof(TestEntity), 4);

        // Act
        var version = registry.GetVersion<TestEntity>();

        // Assert
        Assert.Equal(4, version);
    }
}
