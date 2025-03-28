using System.Linq.Expressions;
using EventSource.Core;
using EventSource.Core.Events;
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

    public Task<T?> ReadByFilterAsync(Expression<Func<T, bool>> filter) =>
        entityStore.GetEntityByFilterAsync(filter);

    public Task<TProjection?> ReadProjectionByIdAsync<TProjection>(
        Guid id,
        Expression<Func<T, TProjection>> projection
    ) => entityStore.GetProjectionByFilterAsync(x => x.Id == id, projection);

    public Task<TProjection?> ReadProjectionByFilterAsync<TProjection>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, TProjection>> projection
    ) => entityStore.GetProjectionByFilterAsync(filter, projection);

    public Task<IReadOnlyCollection<T>> ReadAllAsync() => entityStore.GetAllAsync<T>();

    public Task<IReadOnlyCollection<T>> ReadAllByFilterAsync(Expression<Func<T, bool>> filter) =>
        entityStore.GetAllByFilterAsync(filter);

    public Task<IReadOnlyCollection<TProjection>> ReadAllProjectionsAsync<TProjection>(
        Expression<Func<T, TProjection>> projection
    ) => entityStore.GetAllProjectionsAsync(projection);

    public Task<IReadOnlyCollection<TProjection>> ReadAllProjectionsByFilterAsync<TProjection>(
        Expression<Func<T, TProjection>> projection,
        Expression<Func<T, bool>> filter
    ) => entityStore.GetAllProjectionsByFilterAsync(projection, filter);

    public async Task UpdateAsync(T entity)
    {
        var e = new UpdateEvent<T>(entity);
        using var session = await mongoDbService.StartSessionAsync();
        try
        {
            session.StartTransaction();
            await eventStore.SaveEventAsync(e, session);
            await entityStore.SaveEntityAsync(entity, session);
            await session.CommitTransactionAsync();
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task DeleteAsync(T entity)
    {
        var e = new DeleteEvent<T>(entity);
        using var session = await mongoDbService.StartSessionAsync();
        try
        {
            session.StartTransaction();
            await eventStore.SaveEventAsync(e, session);
            await entityStore.DeleteEntityAsync(entity, session);
            await session.CommitTransactionAsync();
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }
}
