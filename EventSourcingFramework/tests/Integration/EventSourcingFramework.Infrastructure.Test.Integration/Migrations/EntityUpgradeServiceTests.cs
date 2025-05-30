using System.Collections;
using EventSourcingFramework.Application.Abstractions.Migrations;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Test.Utilities.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace EventSourcingFramework.Infrastructure.Test.Integration.Migrations;

[Collection("Integration")]
public class EntityUpgradeServiceTests
{
    private readonly IEntityCollectionNameProvider collectionNameProvider;
    private readonly IMongoDbService mongoDbService;
    private readonly ISchemaVersionRegistry schemaRegistry;
    private readonly IEntityUpgradeService upgradeService;

    public EntityUpgradeServiceTests(
        IEntityUpgradeService upgradeService,
        IMongoDbService mongoDbService,
        IEntityCollectionNameProvider collectionNameProvider,
        ISchemaVersionRegistry schemaRegistry
    )
    {
        this.upgradeService = upgradeService;
        this.mongoDbService = mongoDbService;
        this.collectionNameProvider = collectionNameProvider;
        this.schemaRegistry = schemaRegistry;
    }

    [Fact]
    public async Task MigrateAllEntitiesToLatestVersionAsync_UpgradesV1ToV3()
    {
        // Arrange
        var oldEntity = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe"
        };

        var collectionName = collectionNameProvider.GetCollectionName(typeof(TestEntity));
        var collection = mongoDbService.GetEntityCollection<BsonDocument>(collectionName);
        await collection.DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);

        var bson = oldEntity.ToBsonDocument(typeof(TestEntity1));
        bson["SchemaVersion"] = 1;
        await collection.InsertOneAsync(bson);

        // Act
        await upgradeService.MigrateAllEntitiesToLatestVersionAsync<TestEntity>();

        // Assert
        var all = await collection.Find(_ => true).ToListAsync();
        Assert.Single((IEnumerable)all);

        var upgraded = BsonSerializer.Deserialize<TestEntity>(all.First());
        Assert.Equal("John - Doe", upgraded.Name);
        Assert.Equal(3, upgraded.SchemaVersion);
    }

    [Fact]
    public async Task MigrateAllEntitiesAsync_UpgradesAllRegisteredEntities()
    {
        // Arrange
        var oldEntity = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Smith"
        };

        var collectionName = collectionNameProvider.GetCollectionName(typeof(TestEntity));
        var collection = mongoDbService.GetEntityCollection<BsonDocument>(collectionName);
        await collection.DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);

        var bson = oldEntity.ToBsonDocument(typeof(TestEntity1));
        bson["SchemaVersion"] = 1;
        await collection.InsertOneAsync(bson);

        // Act
        await upgradeService.MigrateAllEntitiesAsync();

        // Assert
        var docs = await collection.Find(_ => true).ToListAsync();
        Assert.Single((IEnumerable)docs);

        var upgraded = BsonSerializer.Deserialize<TestEntity>(docs.First());
        Assert.Equal("Alice - Smith", upgraded.Name);
        Assert.Equal(schemaRegistry.GetVersion<TestEntity>(), upgraded.SchemaVersion);
    }

    [Fact]
    public async Task MigrateAllEntitiesToLatestVersionAsync_IgnoresUpToDateEntities()
    {
        // Arrange
        var latest = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Latest",
            SchemaVersion = 3
        };

        var collectionName = collectionNameProvider.GetCollectionName(typeof(TestEntity));
        var collection = mongoDbService.GetEntityCollection<BsonDocument>(collectionName);
        await collection.DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);

        var bson = latest.ToBsonDocument(typeof(TestEntity));
        bson["SchemaVersion"] = 3;
        await collection.InsertOneAsync(bson);

        // Act
        await upgradeService.MigrateAllEntitiesToLatestVersionAsync<TestEntity>();

        // Assert
        var docs = await collection.Find(_ => true).ToListAsync();
        Assert.Single((IEnumerable)docs);

        var entity = BsonSerializer.Deserialize<TestEntity>(docs.First());
        Assert.Equal("Latest", entity.Name);
        Assert.Equal(3, entity.SchemaVersion);
    }
}