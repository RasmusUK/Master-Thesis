using EventSource.Persistence.Entities;
using MongoDB.Driver;

namespace EventSource.Persistence.Interfaces;

public interface IMongoDbService
{
    IMongoCollection<MongoDbEvent> EventCollection { get; }
    IMongoCollection<MongoDbEntity> EntityCollection { get; }
    IMongoCollection<MongoDbPersonalData> PersonalDataCollection { get; }
    IMongoCollection<TEntity> GetCollection<TEntity>(string collectionName);
    public IMongoDatabase database { get; init; }
}
