using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Infrastructure.MongoDb.Services;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Test.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventSourcingFramework.Infrastructure.Test.Integration.MongoDb;

[Collection("Integration")]
public class ReplayEnvironmentSwitcherTests : MongoIntegrationTestBase
{
    private readonly ReplayEnvironmentSwitcher switcher;

    public ReplayEnvironmentSwitcherTests(IMongoDbService mongoDbService, IReplayContext replayContext)
        : base(mongoDbService, replayContext)
    {
        switcher = new ReplayEnvironmentSwitcher(mongoDbService);
    }

    [Fact]
    public async Task UseDebugDatabaseAsync_WritesToDebugDb()
    {
        // Arrange
        await switcher.UseDebugDatabaseAsync();
        var collection = MongoDbService.GetCollection<BsonDocument>("debug_test_collection");

        var id = Guid.NewGuid().ToString();
        var doc = new BsonDocument { { "_id", id }, { "source", "debug" } };

        // Act
        await collection.InsertOneAsync(doc);
        var found = await collection.Find(Builders<BsonDocument>.Filter.Eq("_id", id)).FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(found);
        Assert.Equal("debug", found["source"]);
    }

    [Fact]
    public async Task UseProductionDatabaseAsync_WritesToProductionDb()
    {
        // Arrange
        await switcher.UseProductionDatabaseAsync();
        var collection = MongoDbService.GetCollection<BsonDocument>("prod_test_collection");

        var id = Guid.NewGuid().ToString();
        var doc = new BsonDocument { { "_id", id }, { "source", "prod" } };

        // Act
        await collection.InsertOneAsync(doc);
        var found = await collection.Find(Builders<BsonDocument>.Filter.Eq("_id", id)).FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(found);
        Assert.Equal("prod", found["source"]);
    }
}