using MongoDB.Driver;

namespace EventSource.Core;

public interface IMongoDbService
{
    IMongoDatabase Database { get; }
}