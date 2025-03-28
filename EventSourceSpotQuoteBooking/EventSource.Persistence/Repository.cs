using System.Linq.Expressions;
using EventSource.Core;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Interfaces;

namespace EventSource.Persistence;

public class Repository<T> : IRepository<T>
    where T : Entity
{
    private readonly IMongoDbEntityStore entityStore;
    private readonly IMongoDbEventStore eventStore;
    private readonly IMongoDbService mongoDbService;

    public Repository(
        IMongoDbEntityStore entityStore,
        IMongoDbEventStore eventStore,
        IMongoDbService mongoDbService
    )
    {
        this.entityStore = entityStore;
        this.eventStore = eventStore;
        this.mongoDbService = mongoDbService;
    }

    public async Task<Guid> CreateAsync(T entity)
    {
        var e = new CreateEvent<T>(entity);
        using var session = await mongoDbService.StartSessionAsync();
        try
        {
            session.StartTransaction();
            await eventStore.SaveEventAsync(e, session);
            await entityStore.SaveEntityAsync(entity, session);
            await session.CommitTransactionAsync();
            return entity.Id;
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public Task<T?> ReadByIdAsync(Guid id) => entityStore.GetEntityByIdAsync<T>(id);

    public Task<T?> ReadByFilterAsync(Expression<Func<T, bool>> filter)
    {
        throw new NotImplementedException();
    }

    public Task<T?> ReadProjectionByFilterAsync<TProjection>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, TProjection>> projection
    )
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<T>> ReadAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<T>> ReadAllByFilterAsync(Expression<Func<T, bool>> filter)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<TProjection>> ReadAllProjectionsAsync<TProjection>(
        Expression<Func<T, TProjection>> projection
    )
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<TProjection>> ReadAllProjectionsByFilterAsync<TProjection>(
        Expression<Func<T, TProjection>> projection,
        Expression<Func<T, bool>> filter
    )
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
