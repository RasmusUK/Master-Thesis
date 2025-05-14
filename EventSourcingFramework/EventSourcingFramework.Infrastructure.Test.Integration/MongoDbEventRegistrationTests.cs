using EventSourcing.Framework.Infrastructure.Shared.Models.Events;
using EventSourcingFramework.Infrastructure.Migrations.Services;
using EventSourcingFramework.Infrastructure.MongoDb.Services;
using EventSourcingFramework.Test.Utilities;
using MongoDB.Bson.Serialization;

namespace EventSourcingFramework.Infrastructure.Test.Integration;

public class MongoDbEventRegistrationTests
{
    [Fact]
    public void RegisterEntities_RegistersMultipleEntitiesWithCorrectNames()
    {
        // Arrange
        var migrationRegistry = new MigrationTypeRegistry();
        var collectionNameProvider = new EntityCollectionNameProvider(migrationRegistry);

        // Act
        MongoDbEventRegistration.RegisterEvents(
            collectionNameProvider,
            (typeof(TestEntity), "TestEntity"),
            (typeof(PersonEntity), "PersonEntity")
        );

        // Assert
        Assert.Equal("TestEntity", collectionNameProvider.GetCollectionName(typeof(TestEntity)));
        Assert.Equal("PersonEntity", collectionNameProvider.GetCollectionName(typeof(PersonEntity)));

        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(MongoCreateEvent<TestEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(MongoUpdateEvent<TestEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(MongoDeleteEvent<TestEntity>)));

        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(MongoCreateEvent<PersonEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(MongoUpdateEvent<PersonEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(MongoDeleteEvent<PersonEntity>)));
    }
    
    [Fact]
    public void RegisterEntities_DoesNotRegisterDuplicateBsonClassMaps()
    {
        // Arrange
        var migrationRegistry = new MigrationTypeRegistry();
        var collectionNameProvider = new EntityCollectionNameProvider(migrationRegistry);

        var entityType = typeof(TestEntity);
        var mappingsBefore = BsonClassMap.IsClassMapRegistered(typeof(MongoCreateEvent<TestEntity>));

        // Act
        MongoDbEventRegistration.RegisterEvents(
            collectionNameProvider,
            (entityType, "TestEntity")
        );

        var mappingsAfterFirst = BsonClassMap.IsClassMapRegistered(typeof(MongoCreateEvent<TestEntity>));
        Assert.True(mappingsAfterFirst);

        MongoDbEventRegistration.RegisterEvents(
            collectionNameProvider,
            (entityType, "TestEntity")
        );

        // Assert
        Assert.Equal("TestEntity", collectionNameProvider.GetCollectionName(entityType));
    }
}