using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Models.Events;
using EventSourcingFramework.Test.Utilities;
using EventSourcingFramework.Test.Utilities.Models;
using MongoDB.Bson.Serialization;

namespace EventSourcingFramework.Infrastructure.Test.Integration.MongoDb;

[Collection("Integration")]
public class MongoDbRegistrationService : MongoIntegrationTestBase
{
    private readonly IMongoDbRegistrationService mongoDbRegistrationService;
    
    public MongoDbRegistrationService(
        IMongoDbService mongoDbService,
        IGlobalReplayContext replayContext, IMongoDbRegistrationService mongoDbRegistrationService)
        : base(mongoDbService, replayContext)
    {
        this.mongoDbRegistrationService = mongoDbRegistrationService;
    }
    [Fact]
    public void RegisterEntities_RegistersMultipleEntitiesWithCorrectNames()
    {
        // Act
        mongoDbRegistrationService.Register(
            (typeof(TestEntity), "TestEntity"),
            (typeof(PersonEntity), "PersonEntity")
        );

        // Assert
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(MongoCreateEvent<TestEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(MongoUpdateEvent<TestEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(MongoDeleteEvent<TestEntity>)));

        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(MongoCreateEvent<PersonEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(MongoUpdateEvent<PersonEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(MongoDeleteEvent<PersonEntity>)));
    }
}