using System.Linq.Expressions;
using EventSource.Core;
using EventSource.Core.Events;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Interfaces;
using MongoDB.Driver;

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
        await eventStore.InsertEventAsync(new CreateEvent<T>(entity));
        await entityStore.InsertEntityAsync(entity);
        return entity.Id;
    }

    public async Task UpdateAsync(T entity)
    {
        await eventStore.InsertEventAsync(new UpdateEvent<T>(entity));
        await entityStore.UpdateEntityAsync(entity);
    }

    public async Task DeleteAsync(T entity)
    {
        await eventStore.InsertEventAsync(new DeleteEvent<T>(entity));
        await entityStore.DeleteEntityAsync(entity);
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

    private async Task ExecuteInTransactionAsync(Func<IClientSessionHandle, Task> action)
    {
        using var session = await mongoDbService.StartSessionAsync();
        session.StartTransaction();

        try
        {
            await action(session);
            await session.CommitTransactionAsync();
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    // public async Task<Guid> CreateAsync(T entity)
    // {
    //     await ExecuteInTransactionAsync(async session =>
    //     {
    //         await eventStore.InsertEventAsync(new CreateEvent<T>(entity), session);
    //         await entityStore.InsertEntityAsync(entity, session);
    //     });
    //
    //     return entity.Id;
    // }
    //
    // public async Task UpdateAsync(T entity)
    // {
    //     await ExecuteInTransactionAsync(async session =>
    //     {
    //         await eventStore.InsertEventAsync(new UpdateEvent<T>(entity), session);
    //         await entityStore.UpdateEntityAsync(entity, session);
    //     });
    // }
    //
    // public async Task DeleteAsync(T entity)
    // {
    //     await ExecuteInTransactionAsync(async session =>
    //     {
    //         await eventStore.InsertEventAsync(new DeleteEvent<T>(entity), session);
    //         await entityStore.DeleteEntityAsync(entity, session);
    //     });
    // }
}
