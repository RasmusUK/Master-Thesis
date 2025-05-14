using EventSourcingFramework.Infrastructure.Shared.Models;
using EventSourcingFramework.Infrastructure.Shared.Models.Events;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventSourcingFramework.Infrastructure.Shared.Interfaces;

public interface IMongoDbService
{
    IMongoCollection<MongoEventBase> EventCollection { get; }
    IMongoCollection<MongoPersonalData> PersonalDataCollection { get; }
    IMongoCollection<MongoApiResponse> ApiResponseCollection { get; }
    IMongoCollection<BsonDocument> CounterCollection { get; }
    IMongoCollection<TEntity> GetEntityCollection<TEntity>(string collectionName);
    IMongoCollection<T> GetCollection<T>(string collectionName, bool alwaysProduction = false);
    IMongoDatabase GetEntityDatabase(bool alwaysProduction = false);
    Task CleanUpAsync();
    Task UseDebugEntityDatabase();
    Task UseProductionEntityDatabase();
}
