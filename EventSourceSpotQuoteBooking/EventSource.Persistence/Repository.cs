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

    public Repository(IMongoDbEntityStore entityStore, IMongoDbEventStore eventStore)
    {
        this.entityStore = entityStore;
        this.eventStore = eventStore;
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

    public async Task<Guid> CreateAsync(T entity, Guid transactionId)
    {
        await eventStore.InsertEventAsync(
            new CreateEvent<T>(entity) { TransactionId = transactionId }
        );
        await entityStore.InsertEntityAsync(entity);
        return entity.Id;
    }

    public async Task UpdateAsync(T entity, Guid transactionId)
    {
        await eventStore.InsertEventAsync(
            new UpdateEvent<T>(entity) { TransactionId = transactionId }
        );
        await entityStore.UpdateEntityAsync(entity);
    }

    public async Task DeleteAsync(T entity, Guid transactionId)
    {
        await eventStore.InsertEventAsync(
            new DeleteEvent<T>(entity) { TransactionId = transactionId }
        );
        await entityStore.DeleteEntityAsync(entity);
    }

    public async Task<Guid> CreateCompensationAsync(T entity, Guid transactionId)
    {
        await eventStore.InsertEventAsync(
            new CreateEvent<T>(entity) { TransactionId = transactionId, Compensation = true }
        );
        await entityStore.InsertEntityAsync(entity);
        return entity.Id;
    }

    public async Task UpdateCompensationAsync(T entity, Guid transactionId)
    {
        await eventStore.InsertEventAsync(
            new UpdateEvent<T>(entity) { TransactionId = transactionId, Compensation = true }
        );
        await entityStore.UpdateEntityAsync(entity);
    }

    public async Task DeleteCompensationAsync(T entity, Guid transactionId)
    {
        await eventStore.InsertEventAsync(
            new DeleteEvent<T>(entity) { TransactionId = transactionId, Compensation = true }
        );
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
}
