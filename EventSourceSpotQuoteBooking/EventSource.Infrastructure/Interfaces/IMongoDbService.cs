using EventSource.Persistence.Entities;
using MongoDB.Driver;

namespace EventSource.Persistence.Interfaces;

public interface IMongoDbService
{
    IMongoCollection<MongoDbEvent> EventCollection { get; }
    IMongoCollection<MongoDbEntity> EntityCollection { get; }
}
