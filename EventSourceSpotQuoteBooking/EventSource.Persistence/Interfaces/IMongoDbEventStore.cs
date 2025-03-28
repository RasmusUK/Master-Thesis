using EventSource.Core;
using MongoDB.Driver;

namespace EventSource.Persistence.Interfaces;

public interface IMongoDbEventStore
{
    Task SaveEventAsync(Event e, IClientSessionHandle session);
}
