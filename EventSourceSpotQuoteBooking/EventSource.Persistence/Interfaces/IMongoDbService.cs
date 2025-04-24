using EventSource.Persistence.Events;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventSource.Persistence.Interfaces;

public interface IMongoDbService
{
    IMongoCollection<EventBase> EventCollection { get; }
    IMongoCollection<BsonDocument> CounterCollection { get; }
    IMongoCollection<TEntity> GetEntityCollection<TEntity>(string collectionName);
    Task CleanUpAsync();
}
