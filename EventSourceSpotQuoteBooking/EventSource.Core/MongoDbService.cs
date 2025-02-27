using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EventSource.Core;

public class MongoDbService : IMongoDbService
{
    public IMongoDatabase Database { get; }

    public MongoDbService(IOptions<MongoDbOptions> mongoDbOptions)
    {
        var eventStoreOptions = mongoDbOptions.Value.EventStore;
        if (string.IsNullOrEmpty(eventStoreOptions.ConnectionString))
            throw new ArgumentException("MongoDb connection string is required");
        if (string.IsNullOrEmpty(eventStoreOptions.DatabaseName))
            throw new ArgumentException("MongoDb database name is required");
        
        var mongoUrl = MongoUrl.Create(eventStoreOptions.ConnectionString);
        var mongoClient = new MongoClient(mongoUrl);
        Database = mongoClient.GetDatabase(eventStoreOptions.DatabaseName);
    }
}