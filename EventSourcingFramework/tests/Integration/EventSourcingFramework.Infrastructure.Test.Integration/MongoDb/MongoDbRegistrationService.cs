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
        IReplayContext replayContext, IMongoDbRegistrationService mongoDbRegistrationService)
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
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(CreateEvent<TestEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(UpdateEvent<TestEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(DeleteEvent<TestEntity>)));

        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(CreateEvent<PersonEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(UpdateEvent<PersonEntity>)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(DeleteEvent<PersonEntity>)));
    }
}