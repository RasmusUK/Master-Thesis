using System.Net;
using System.Text;
using System.Text.Json;
using EventSourcingFramework.Application.Abstractions.ApiGateway;
using EventSourcingFramework.Application.Abstractions.EventStore;
using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Core.Enums;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Http.Services;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Models;
using EventSourcingFramework.Infrastructure.Stores.EventStore;
using EventSourcingFramework.Test.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Moq.Protected;

namespace EventSourcingFramework.Infrastructure.Test.Integration.Http;

[Collection("Integration")]
public class ApiGatewayTests : MongoIntegrationTestBase
{
    private readonly IApiGateway apiGateway;
    private readonly IMongoCollection<MongoApiResponse> collection;
    private readonly IReplayContext replayContext;
    private readonly IEventSequenceGenerator sequenceGenerator;

    public ApiGatewayTests(IMongoDbService mongoDbService, IReplayContext replayContext,
        IApiResponseStore responseStore, IEventSequenceGenerator sequenceGenerator)
        : base(mongoDbService, replayContext)
    {
        this.replayContext = replayContext;
        this.sequenceGenerator = sequenceGenerator;
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

        apiGateway = new ApiGateway(httpClient, responseStore, replayContext, sequenceGenerator);
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

        replayContext.StartReplay();

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

        replayContext.StartReplay();

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
    }

    [Fact]
    public async Task PostAsync_CallsHttpClient_AndStoresResponse()
    {
        // Arrange
        var url = "https://fake.api/post-endpoint";
        var requestBody = new { Name = "NewPost", Age = 30 };
        var key = $"POST:{url}:{JsonSerializer.Serialize(requestBody)}";

        await collection.DeleteManyAsync(x => x.Key == key);

        // Act
        var result = await apiGateway.PostAsync<object, SampleResponse>(url, requestBody);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Live Response", result.Message);
        Assert.Equal(123, result.Value);

        var stored = await collection.Find(x => x.Key == key).FirstOrDefaultAsync();
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task SendAsync_CallsHttpClient_AndStoresResponse()
    {
        // Arrange
        var url = "https://fake.api/custom";
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { Data = "X" }), Encoding.UTF8, "application/json")
        };

        var content = await request.Content.ReadAsStringAsync();
        var key = $"PUT:{url}:{content}";

        await collection.DeleteManyAsync(x => x.Key == key);

        // Act
        var result = await apiGateway.SendAsync<SampleResponse>(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Live Response", result.Message);
        Assert.Equal(123, result.Value);

        var stored = await collection.Find(x => x.Key == key).FirstOrDefaultAsync();
        Assert.NotNull(stored);
    }

    private class SampleResponse
    {
        public string Message { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}