using System.Net;
using System.Text;
using System.Text.Json;
using EventSourcingFramework.Application.Abstractions.ApiGateway;
using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Application.UseCases.ReplayContext;
using EventSourcingFramework.Core.Enums;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Http.Services;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Models;
using EventSourcingFramework.Infrastructure.Stores.ApiResponseStore;
using EventSourcingFramework.Test.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Moq.Protected;

namespace EventSourcingFramework.Infrastructure.Test.Integration;

[Collection("Integration")]
public class ApiGatewayTests : MongoIntegrationTestBase
{
    private readonly IApiGateway apiGateway;
    private readonly IGlobalReplayContext replayContext;
    private readonly IMongoCollection<MongoApiResponse> collection;

    public ApiGatewayTests(IMongoDbService mongoDbService, IGlobalReplayContext replayContext, IApiResponseStore responseStore)
        : base(mongoDbService, replayContext)
    {
        this.replayContext = replayContext;
        collection = mongoDbService.ApiResponseCollection;

        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage _, CancellationToken _) =>
            {
                var json = JsonSerializer.Serialize(new SampleResponse
                {
                    Message = "Live Response",
                    Value = 123
                });

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://fake.api/")
        };

        apiGateway = new ApiGateway(httpClient, responseStore, replayContext);
    }

    private class SampleResponse
    {
        public string Message { get; set; } = string.Empty;
        public int Value { get; set; }
    }
    
    [Fact]
    public async Task GetAsync_NoReplay_CallsHttpAndStoresResponse()
    {
        // Arrange
        var url = "https://fake.api/test";
        var key = $"GET:{url}";
        await collection.DeleteManyAsync(x => x.Key == key);
        
        // Act
        var result = await apiGateway.GetAsync<SampleResponse>(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Live Response", result.Message);
        Assert.Equal(123, result.Value);

        var stored = await collection.Find(x => x.Key == key).FirstOrDefaultAsync();
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task GetAsync_ExternalOnlyMode_CallsHttpAndStoresResponse()
    {
        // Arrange
        var url = "https://fake.api/test";
        var key = $"GET:{url}";
        await collection.DeleteManyAsync(x => x.Key == key);
        
        replayContext.StartReplay(ReplayMode.Strict, ApiReplayMode.ExternalOnly);
        
        // Act
        var result = await apiGateway.GetAsync<SampleResponse>(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Live Response", result.Message);
        Assert.Equal(123, result.Value);

        var stored = await collection.Find(x => x.Key == key).FirstOrDefaultAsync();
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task GetAsync_CacheOnlyMode_ReturnsCachedResponse()
    {
        // Arrange
        var url = "https://fake.api/cached";
        var key = $"GET:{url}";
        var expected = new SampleResponse { Message = "FromCache", Value = 999 };

        await collection.InsertOneAsync(new MongoApiResponse
        {
            Key = key,
            Response = expected.ToBsonDocument(),
            CreatedAt = DateTime.UtcNow
        });

        replayContext.StartReplay(ReplayMode.Strict, ApiReplayMode.CacheOnly);

        // Act
        var result = await apiGateway.GetAsync<SampleResponse>(url);

        // Assert
        Assert.Equal(expected.Message, result.Message);
        Assert.Equal(expected.Value, result.Value);
    }

    [Fact]
    public async Task GetAsync_CacheOnlyMode_ThrowsIfNotCached()
    {
        // Arrange
        var url = "https://fake.api/missing";
        var key = $"GET:{url}";
        await collection.DeleteManyAsync(x => x.Key == key);

        replayContext.StartReplay(ReplayMode.Strict, ApiReplayMode.CacheOnly);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            apiGateway.GetAsync<SampleResponse>(url));
        Assert.Contains("No cached response", ex.Message);
    }

    [Fact]
    public async Task GetAsync_CacheThenExternal_UsesCacheIfAvailable()
    {
        // Arrange
        var url = "https://fake.api/fallback";
        var key = $"GET:{url}";
        var expected = new SampleResponse { Message = "Cached", Value = 555 };

        await collection.InsertOneAsync(new MongoApiResponse
        {
            Key = key,
            Response = expected.ToBsonDocument(),
            CreatedAt = DateTime.UtcNow
        });

        replayContext.StartReplay(ReplayMode.Strict, ApiReplayMode.CacheThenExternal);

        // Act
        var result = await apiGateway.GetAsync<SampleResponse>(url);

        // Assert
        Assert.Equal(expected.Message, result.Message);
        Assert.Equal(expected.Value, result.Value);
    }

    [Fact]
    public async Task GetAsync_CacheThenExternal_CallsHttpIfNotCached()
    {
        // Arrange
        var url = "https://fake.api/live";
        var key = $"GET:{url}";
        await collection.DeleteManyAsync(x => x.Key == key);

        replayContext.StartReplay(ReplayMode.Strict, ApiReplayMode.CacheThenExternal);

        // Act
        var result = await apiGateway.GetAsync<SampleResponse>(url);

        // Assert
        Assert.Equal("Live Response", result.Message);
        Assert.Equal(123, result.Value);

        var stored = await collection.Find(x => x.Key == key).FirstOrDefaultAsync();
        Assert.NotNull(stored);
    }
}