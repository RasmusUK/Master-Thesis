using EventSource.Persistence.Entities;
using EventSource.Persistence.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EventSource.Persistence;

public class MongoDbService : IMongoDbService
{
    public IMongoCollection<MongoDbEvent> EventCollection { get; }
    public IMongoCollection<MongoDbEntity> EntityCollection { get; }
    public IMongoCollection<MongoDbPersonalData> PersonalDataCollection { get; }

    public MongoDbService(IOptions<MongoDbOptions> mongoDbOptions)
    {
        var eventStoreOptions = mongoDbOptions.Value.EventStore;
        if (string.IsNullOrEmpty(eventStoreOptions.ConnectionString))
            throw new ArgumentException("MongoDb connection string is required");
        if (string.IsNullOrEmpty(eventStoreOptions.DatabaseName))
            throw new ArgumentException("MongoDb database name is required");

        var mongoUrl = MongoUrl.Create(eventStoreOptions.ConnectionString);
        var mongoClient = new MongoClient(mongoUrl);
        var database = mongoClient.GetDatabase(eventStoreOptions.DatabaseName);

        EventCollection = database.GetCollection<MongoDbEvent>("events");
        EntityCollection = database.GetCollection<MongoDbEntity>("entities");
        PersonalDataCollection = database.GetCollection<MongoDbPersonalData>("personalInfos");
    }
}
