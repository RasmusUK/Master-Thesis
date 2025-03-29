using EventSource.Core;
using MongoDB.Driver;

namespace EventSource.Persistence.Interfaces;

public interface IMongoDbEventStore
{
    Task InsertEventAsync(Event e, IClientSessionHandle session);
}
