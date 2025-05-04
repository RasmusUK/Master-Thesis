using EventSource.Application.Interfaces;
using EventSource.Persistence.Interfaces;
using EventSource.Test.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventSource.Persistence.Test.Integration;

[Collection("Integration")]
public class MongoDbServiceTests : MongoIntegrationTestBase
{
    private readonly IMongoDbService mongoDbService;
    private readonly IEntityCollectionNameProvider nameProvider;

    public MongoDbServiceTests(
        IMongoDbService mongoDbService,
        IGlobalReplayContext replayContext,
        IEntityCollectionNameProvider nameProvider
    )
        : base(mongoDbService, replayContext)
    {
        this.mongoDbService = mongoDbService;
        this.nameProvider = nameProvider;
    }

    [Fact]
    public async Task CounterCollection_CanInsertAndRetrieve()
    {
        // Arrange
        var collection = mongoDbService.CounterCollection;
        var doc = new BsonDocument { { "_id", "testCounter" }, { "seq", 1 } };
        await collection.InsertOneAsync(doc);

        // Act
        var fetched = await collection
            .Find(Builders<BsonDocument>.Filter.Eq("_id", "testCounter"))
            .FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(1, fetched["seq"].AsInt32);
    }

    [Fact]
    public async Task UseDebugEntityDatabase_ClonesProductionData()
    {
        // Arrange
        const string collectionName = "test_entities";
        nameProvider.Register(typeof(TestEntity), collectionName);

        var prodCollection = mongoDbService.GetCollection<TestEntity>(
            collectionName,
            alwaysProduction: true
        );
        await prodCollection.InsertOneAsync(
            new TestEntity { Id = Guid.NewGuid(), Name = "ProdEntity" }
        );

        // Act
        await mongoDbService.UseDebugEntityDatabase();
        var debugCollection = mongoDbService.GetCollection<TestEntity>(collectionName);
        var debugData = await debugCollection.Find(_ => true).ToListAsync();

        // Assert
        Assert.Single(debugData);
        Assert.Equal("ProdEntity", debugData[0].Name);
    }

    [Fact]
    public async Task UseProductionEntityDatabase_SwitchesBackAndCleansDebug()
    {
        // Arrange
        await mongoDbService.UseDebugEntityDatabase();

        // Act
        await mongoDbService.UseProductionEntityDatabase();
        var debugDb = mongoDbService.GetEntityDatabase(alwaysProduction: false);
        var debugCollections = await debugDb.ListCollectionNames().ToListAsync();

        // Assert
        Assert.Empty(debugCollections);
    }

    [Fact]
    public async Task CleanUpAsync_RemovesAllDatabases()
    {
        // Arrange
        const string collectionName = "test_entities";
        nameProvider.Register(typeof(TestEntity), collectionName);

        var prodCollection = mongoDbService.GetCollection<TestEntity>(collectionName, true);
        var debugCollection = mongoDbService.GetCollection<TestEntity>(collectionName);
        var personalDb = mongoDbService.GetCollection<BsonDocument>("personal_data");

        await prodCollection.InsertOneAsync(
            new TestEntity { Id = Guid.NewGuid(), Name = "ProdData" }
        );
        await debugCollection.InsertOneAsync(
            new TestEntity { Id = Guid.NewGuid(), Name = "DebugData" }
        );
        await personalDb.InsertOneAsync(new BsonDocument("test", 1));

        // Act
        await mongoDbService.CleanUpAsync();

        var prodData = await prodCollection.Find(_ => true).ToListAsync();
        var debugData = await debugCollection.Find(_ => true).ToListAsync();

        // Assert
        Assert.Empty(prodData);
        Assert.Empty(debugData);
    }
}
