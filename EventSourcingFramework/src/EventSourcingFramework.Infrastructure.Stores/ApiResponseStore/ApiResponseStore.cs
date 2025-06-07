using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Models;
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

    public async Task<T?> GetAsync<T>(string key, long eventNumber)
    {
        var filter = Builders<MongoApiResponse>.Filter.And(
            Builders<MongoApiResponse>.Filter.Eq(x => x.Key, key),
            Builders<MongoApiResponse>.Filter.Lte(x => x.EventNumber, eventNumber)
        );

        var doc = await collection
            .Find(filter)
            .SortByDescending(x => x.EventNumber)
            .FirstOrDefaultAsync();

        return doc is not null 
            ? BsonSerializer.Deserialize<T>(doc.Response) 
            : default;
    }

    public async Task SaveAsync<T>(string key, long eventNumber, T response)
    {
        var document = new MongoApiResponse
        {
            Key = key,
            Response = response.ToBsonDocument(),
            CreatedAt = DateTime.UtcNow,
            EventNumber = eventNumber
        };

        await collection.InsertOneAsync(document);
    }
}