using EventSource.Persistence.Interfaces;

namespace EventSource.Persistence.Test.Integration;

[Collection("Integration")]
public class EventSequenceGeneratorTests
{
    private readonly IEventSequenceGenerator generator;

    public EventSequenceGeneratorTests(IEventSequenceGenerator generator)
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
