using EventSource.Core;
using EventSource.Core.Interfaces;
using MongoDB.Driver;

namespace EventSource.Persistence.Interfaces;

public interface IMongoDbEventStore : IEventStore
{
    Task InsertEventAsync(Event e, IClientSessionHandle session);
}
