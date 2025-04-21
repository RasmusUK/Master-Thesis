using EventSource.Persistence.Entities;
using MongoDB.Driver;

namespace EventSource.Persistence.Interfaces;

public interface IMongoDbService
{
    IMongoCollection<MongoDbEvent> EventCollection { get; }
    IMongoCollection<MongoDbPersonalData> PersonalDataCollection { get; }
    IMongoCollection<TEntity> GetEntityCollection<TEntity>(string collectionName);
    Task CleanUpAsync();
}
