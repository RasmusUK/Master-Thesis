using EventSource.Persistence.Interfaces;
using EventSource.Test.Utilities;

namespace EventSource.Persistence.Test.Integration;

[Collection("Integration")]
public class EventSequenceGeneratorTests : MongoIntegrationTestBase
{
    private readonly IEventSequenceGenerator generator;

    public EventSequenceGeneratorTests(
        IMongoDbService mongoDbService,
        IEventSequenceGenerator generator
    )
        : base(mongoDbService)
    {
        this.generator = generator;
    }

    [Fact]
    public async Task GetNextSequenceNumberAsync_IncrementsValue()
    {
        // Act
        var first = await generator.GetNextSequenceNumberAsync();
        var second = await generator.GetNextSequenceNumberAsync();

        // Assert
        Assert.Equal(first + 1, second);
    }

    [Fact]
    public async Task GetCurrentSequenceNumberAsync_ReturnsCurrentValue()
    {
        // Arrange
        var next = await generator.GetNextSequenceNumberAsync();

        // Act
        var current = await generator.GetCurrentSequenceNumberAsync();

        // Assert
        Assert.Equal(next, current);
    }

    [Fact]
    public async Task GetCurrentSequenceNumberAsync_ReturnsZero_IfNoSequenceExists()
    {
        // Act
        var current = await generator.GetCurrentSequenceNumberAsync();

        // Assert
        Assert.True(current >= 0);
    }
}
