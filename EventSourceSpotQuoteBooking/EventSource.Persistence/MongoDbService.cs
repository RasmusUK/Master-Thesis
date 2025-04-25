using System.Reflection;
using EventSource.Persistence.Events;
using EventSource.Persistence.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace EventSource.Persistence;

public class MongoDbService : IMongoDbService
{
    public IMongoCollection<EventBase> EventCollection { get; }
    public IMongoCollection<BsonDocument> CounterCollection { get; }
    private IMongoDatabase entityDatabase;
    private readonly IMongoDatabase eventDatabase;
    private readonly IMongoDatabase productionEntityDatabase;
    private readonly IMongoDatabase debugEntityDatabase;
    private readonly IMongoDatabase personalDataDatabase;
    private readonly MongoClient eventClient;
    private readonly MongoClient productionEntityClient;
    private readonly MongoClient debugEntityClient;
    private readonly MongoClient personalDataClient;
    private readonly IEntityCollectionNameProvider entityCollectionNameProvider;

    public MongoDbService(
        IOptions<MongoDbOptions> mongoDbOptions,
        IEntityCollectionNameProvider entityCollectionNameProvider
    )
    {
        this.entityCollectionNameProvider = entityCollectionNameProvider;
        RegisterConventions();

        var options = mongoDbOptions.Value;

        (eventDatabase, eventClient) = CreateDatabase(options.EventStore);
        (productionEntityDatabase, productionEntityClient) = CreateDatabase(options.EntityStore);
        (debugEntityDatabase, debugEntityClient) = CreateDatabase(options.DebugEntityStore);
        (personalDataDatabase, personalDataClient) = CreateDatabase(options.PersonalDataStore);

        EventCollection = eventDatabase.GetCollection<EventBase>("events");
        CounterCollection = eventDatabase.GetCollection<BsonDocument>("counters");

        entityDatabase = productionEntityDatabase;

        EnsureEventIndexesAsync().GetAwaiter().GetResult();
    }

    public IMongoCollection<TEntity> GetEntityCollection<TEntity>(string collectionName) =>
        entityDatabase.GetCollection<TEntity>(collectionName);

    public IMongoCollection<T> GetCollection<T>(
        string collectionName,
        bool alwaysProduction = false
    ) =>
        alwaysProduction
            ? productionEntityDatabase.GetCollection<T>(collectionName)
            : entityDatabase.GetCollection<T>(collectionName);

    public IMongoDatabase GetEntityDatabase(bool alwaysProduction = false) =>
        alwaysProduction ? productionEntityDatabase : entityDatabase;

    public async Task CleanUpAsync()
    {
        await eventClient.DropDatabaseAsync(eventDatabase.DatabaseNamespace.DatabaseName);
        await productionEntityClient.DropDatabaseAsync(
            productionEntityDatabase.DatabaseNamespace.DatabaseName
        );
        await personalDataClient.DropDatabaseAsync(
            personalDataDatabase.DatabaseNamespace.DatabaseName
        );
        await debugEntityClient.DropDatabaseAsync(
            debugEntityDatabase.DatabaseNamespace.DatabaseName
        );
    }

    public async Task UseDebugEntityDatabase()
    {
        entityDatabase = debugEntityDatabase;
        await CloneProductionToReplayAsync();
    }

    public async Task UseProductionEntityDatabase()
    {
        entityDatabase = productionEntityDatabase;
        await debugEntityClient.DropDatabaseAsync(
            debugEntityDatabase.DatabaseNamespace.DatabaseName
        );
    }

    private async Task CloneProductionToReplayAsync()
    {
        var registered = entityCollectionNameProvider.GetAllRegistered();

        foreach (var (type, collectionName) in registered)
        {
            var method = typeof(MongoDbService).GetMethod(
                nameof(CopyCollectionAsync),
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            var genericMethod = method!.MakeGenericMethod(type);
            await (Task)genericMethod.Invoke(this, new object[] { collectionName })!;
        }
    }

    private async Task CopyCollectionAsync<T>(string collectionName)
    {
        var source = productionEntityDatabase.GetCollection<T>(collectionName);
        var target = debugEntityDatabase.GetCollection<T>(collectionName);

        var allDocs = await source.Find(_ => true).ToListAsync();
        await target.InsertManyAsync(allDocs);
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

        var eventNumberIndexExists = existingList.Any(index =>
            index.GetValue("name", "").AsString == "EventNumber_Ascending"
        );

        if (!eventNumberIndexExists)
        {
            var indexModel = new CreateIndexModel<EventBase>(
                Builders<EventBase>.IndexKeys.Ascending(e => e.EventNumber),
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
            if (memberMap.MemberType == typeof(Guid))
            {
                memberMap.SetSerializer(new GuidSerializer(BsonType.String));
            }
            else if (memberMap.MemberType == typeof(Guid?))
            {
                memberMap.SetSerializer(
                    new NullableSerializer<Guid>(new GuidSerializer(BsonType.String))
                );
            }
        }
    }
}
