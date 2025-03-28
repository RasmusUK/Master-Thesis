using EventSource.Core;
using EventSource.Core.Interfaces;
using MongoDB.Driver;

namespace EventSource.Persistence.Interfaces;

public interface IMongoDbEntityStore : IEntityStore
{
    Task SaveEntityAsync<TEntity>(TEntity entity, IClientSessionHandle session)
        where TEntity : Entity;
}
