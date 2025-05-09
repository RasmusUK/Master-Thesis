using EventSourcingFramework.Persistence.Events;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventSourcingFramework.Persistence.Interfaces;

public interface IMongoDbService
{
    IMongoCollection<EventBase> EventCollection { get; }
    IMongoCollection<PersonalData> PersonalDataCollection { get; }
    IMongoCollection<BsonDocument> CounterCollection { get; }
    IMongoCollection<TEntity> GetEntityCollection<TEntity>(string collectionName);
    IMongoCollection<T> GetCollection<T>(string collectionName, bool alwaysProduction = false);
    IMongoDatabase GetEntityDatabase(bool alwaysProduction = false);
    Task CleanUpAsync();
    Task UseDebugEntityDatabase();
    Task UseProductionEntityDatabase();
}
