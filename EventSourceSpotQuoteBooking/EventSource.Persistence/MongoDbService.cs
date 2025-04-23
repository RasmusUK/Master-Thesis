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
    public IMongoCollection<BsonDocument> CounterCollection { get; }
    private IMongoDatabase EventDatabase { get; }
    private IMongoDatabase EntityDatabase { get; }
    private IMongoDatabase PersonalDataDatabase { get; }
    private MongoClient EventClient { get; }
    private MongoClient EntityClient { get; }
    private MongoClient PersonalDataClient { get; }

    public MongoDbService(IOptions<MongoDbOptions> mongoDbOptions)
    {
        RegisterConventions();

        var options = mongoDbOptions.Value;

        (EventDatabase, EventClient) = CreateDatabase(options.EventStore);
        (EntityDatabase, EntityClient) = CreateDatabase(options.EntityStore);
        (PersonalDataDatabase, PersonalDataClient) = CreateDatabase(options.PersonalDataStore);

        EventCollection = EventDatabase.GetCollection<MongoDbEvent>("events");
        PersonalDataCollection = PersonalDataDatabase.GetCollection<MongoDbPersonalData>(
            "personalData"
        );
        CounterCollection = EventDatabase.GetCollection<BsonDocument>("counters");

        EnsureEventIndexesAsync().GetAwaiter().GetResult();
    }

    public IMongoCollection<TEntity> GetEntityCollection<TEntity>(string collectionName) =>
        EntityDatabase.GetCollection<TEntity>(collectionName);

    public async Task CleanUpAsync()
    {
        await EventClient.DropDatabaseAsync(EventDatabase.DatabaseNamespace.DatabaseName);
        await EntityClient.DropDatabaseAsync(EntityDatabase.DatabaseNamespace.DatabaseName);
        await PersonalDataClient.DropDatabaseAsync(
            PersonalDataDatabase.DatabaseNamespace.DatabaseName
        );
    }

    private static (IMongoDatabase db, MongoClient client) CreateDatabase(DatabaseOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new ArgumentException("MongoDB connection string is required.");

        if (string.IsNullOrWhiteSpace(options.DatabaseName))
            throw new ArgumentException("MongoDB database name is required.");

        var client = new MongoClient(MongoUrl.Create(options.ConnectionString));
        var db = client.GetDatabase(options.DatabaseName);
        return (db, client);
    }

    private static void RegisterConventions()
    {
        var conventionPack = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true),
            new GuidAsStringConvention(),
        };

        ConventionRegistry.Register("GlobalConventions", conventionPack, _ => true);
    }

    private async Task EnsureEventIndexesAsync()
    {
        var existingIndexes = await EventCollection.Indexes.ListAsync();
        var existingList = await existingIndexes.ToListAsync();

        if (existingList.All(index => index["name"] != "EventNumber_Ascending"))
        {
            var indexModel = new CreateIndexModel<MongoDbEvent>(
                Builders<MongoDbEvent>.IndexKeys.Ascending(e => e.EventNumber),
                new CreateIndexOptions { Unique = true, Name = "EventNumber_Ascending" }
            );

            await EventCollection.Indexes.CreateOneAsync(indexModel);
        }
    }

    private class GuidAsStringConvention : IMemberMapConvention
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
}
