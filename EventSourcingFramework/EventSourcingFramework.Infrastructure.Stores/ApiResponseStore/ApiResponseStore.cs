using EventSourcing.Framework.Infrastructure.Shared.Interfaces;
using EventSourcing.Framework.Infrastructure.Shared.Models;
using EventSourcingFramework.Core.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace EventSourcingFramework.Infrastructure.Stores.ApiResponseStore;

public class ApiResponseStore : IApiResponseStore
{
    private readonly IMongoCollection<MongoApiResponse> collection;

    public ApiResponseStore(IMongoDbService mongoDbService)
    {
        collection = mongoDbService.ApiResponseCollection;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var filter = Builders<MongoApiResponse>.Filter.Eq(x => x.Key, key);
        var doc = await collection.Find(filter).FirstOrDefaultAsync();

        return doc is not null ? BsonSerializer.Deserialize<T>(doc.Response) : default;
    }

    public async Task SaveAsync<T>(string key, T response)
    {
        var document = new MongoApiResponse
        {
            Key = key,
            Response = response.ToBsonDocument(),
            CreatedAt = DateTime.UtcNow
        };

        await collection.InsertOneAsync(document);
    }
}