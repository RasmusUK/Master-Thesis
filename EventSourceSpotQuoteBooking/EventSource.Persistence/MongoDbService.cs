using EventSource.Persistence.Entities;
using EventSource.Persistence.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace EventSource.Persistence;

public class MongoDbService : IMongoDbService
{
    public IMongoCollection<MongoDbEvent> EventCollection { get; }
    public IMongoCollection<MongoDbPersonalData> PersonalDataCollection { get; }
    public IMongoDatabase database { get; init; }

    public MongoDbService(IOptions<MongoDbOptions> mongoDbOptions)
    {
        var conventionPack = new ConventionPack { new GuidAsStringConvention() };
        ConventionRegistry.Register("GuidAsString", conventionPack, _ => true);

        var eventStoreOptions = mongoDbOptions.Value.EventStore;
        if (string.IsNullOrEmpty(eventStoreOptions.ConnectionString))
            throw new ArgumentException("MongoDb connection string is required");
        if (string.IsNullOrEmpty(eventStoreOptions.DatabaseName))
            throw new ArgumentException("MongoDb database name is required");

        var mongoUrl = MongoUrl.Create(eventStoreOptions.ConnectionString);
        var mongoClient = new MongoClient(mongoUrl);
        database = mongoClient.GetDatabase(eventStoreOptions.DatabaseName);

        EventCollection = database.GetCollection<MongoDbEvent>("events");
        PersonalDataCollection = database.GetCollection<MongoDbPersonalData>("personalData");
    }

    public IMongoCollection<TEntity> GetCollection<TEntity>(string collectionName) =>
        database.GetCollection<TEntity>(collectionName);
}

public class GuidAsStringConvention : IMemberMapConvention
{
    public string Name => "GuidAsString";

    public void Apply(BsonMemberMap memberMap)
    {
        if (memberMap.MemberType == typeof(Guid) || memberMap.MemberType == typeof(Guid?))
        {
            memberMap.SetSerializer(
                new MongoDB.Bson.Serialization.Serializers.GuidSerializer(BsonType.String)
            );
        }
    }
}
