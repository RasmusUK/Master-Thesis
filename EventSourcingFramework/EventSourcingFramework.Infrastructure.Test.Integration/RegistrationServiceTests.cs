using EventSourcingFramework.Persistence;
using EventSourcingFramework.Persistence.Events;
using EventSourcingFramework.Test.Utilities;
using MongoDB.Bson.Serialization;

namespace EventSourcingFramework.Infrastructure.Test.Integration;

public class RegistrationServiceTests
{
    [Fact]
    public void RegisterEntities_RegistersMultipleEntitiesWithCorrectNames()
    {
        // Arrange
        var migrationRegistry = new MigrationTypeRegistry();
        var collectionNameProvider = new EntityCollectionNameProvider(migrationRegistry);

        // Act
        RegistrationService.RegisterEntities(
            collectionNameProvider,
            (typeof(TestEntity), "TestEntity"),
            (typeof(PersonEntity), "PersonEntity")
        );

        // Assert
        Assert.Equal("TestEntity", collectionNameProvider.GetCollectionName(typeof(TestEntity)));
        Assert.Equal("PersonEntity", collectionNameProvider.GetCollectionName(typeof(PersonEntity)));

        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(CreateEvent<TestEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(UpdateEvent<TestEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(DeleteEvent<TestEntity>)));

        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(CreateEvent<PersonEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(UpdateEvent<PersonEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(DeleteEvent<PersonEntity>)));
    }
    
    [Fact]
    public void RegisterEntities_DoesNotRegisterDuplicateBsonClassMaps()
    {
        // Arrange
        var migrationRegistry = new MigrationTypeRegistry();
        var collectionNameProvider = new EntityCollectionNameProvider(migrationRegistry);

        var entityType = typeof(TestEntity);
        var mappingsBefore = BsonClassMap.IsClassMapRegistered(typeof(CreateEvent<TestEntity>));

        // Act
        RegistrationService.RegisterEntities(
            collectionNameProvider,
            (entityType, "TestEntity")
        );

        var mappingsAfterFirst = BsonClassMap.IsClassMapRegistered(typeof(CreateEvent<TestEntity>));
        Assert.True(mappingsAfterFirst);

        RegistrationService.RegisterEntities(
            collectionNameProvider,
            (entityType, "TestEntity")
        );

        // Assert
        Assert.Equal("TestEntity", collectionNameProvider.GetCollectionName(entityType));
    }
}