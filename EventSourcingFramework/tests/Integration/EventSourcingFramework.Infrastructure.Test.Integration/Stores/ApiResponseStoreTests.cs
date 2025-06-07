using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Test.Utilities;

namespace EventSourcingFramework.Infrastructure.Test.Integration.Stores;

[Collection("Integration")]
public class ApiResponseStoreTests : MongoIntegrationTestBase
{
    private readonly IApiResponseStore store;

    public ApiResponseStoreTests(IMongoDbService mongoDbService, IReplayContext replayContext,
        IApiResponseStore store)
        : base(mongoDbService, replayContext)
    {
        this.store = store;
    }

    [Fact]
    public async Task SaveAsync_Then_GetAsync_Returns_Same_Data()
    {
        // Arrange
        var key = $"test:{Guid.NewGuid()}";
        var expected = new SampleResponse { Message = "Hello", Value = 42 };

        // Act
        await store.SaveAsync(key, 1, expected);
        var result = await store.GetAsync<SampleResponse>(key, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.Message, result.Message);
        Assert.Equal(expected.Value, result.Value);
    }

    [Fact]
    public async Task GetAsync_NonExistentKey_ReturnsNull()
    {
        // Arrange
        var nonExistentKey = $"missing:{Guid.NewGuid()}";

        // Act
        var result = await store.GetAsync<SampleResponse>(nonExistentKey, 1);

        // Assert
        Assert.Null(result);
    }

    private class SampleResponse
    {
        public string Message { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}