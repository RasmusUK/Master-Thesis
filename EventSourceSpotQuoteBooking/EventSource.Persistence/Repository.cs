using System.Linq.Expressions;
using EventSource.Application.Interfaces;
using EventSource.Core;
using EventSource.Core.Events;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Events;

namespace EventSource.Persistence;

public class Repository<T> : IRepository<T>
    where T : IEntity
{
    private readonly IEntityStore entityStore;
    private readonly IEventStore eventStore;
    private readonly IGlobalReplayContext globalReplayContext;

    public Repository(
        IEntityStore entityStore,
        IEventStore eventStore,
        IGlobalReplayContext globalReplayContext
    )
    {
        this.entityStore = entityStore;
        this.eventStore = eventStore;
        this.globalReplayContext = globalReplayContext;
    }

    public async Task<Guid> CreateAsync(T entity)
    {
        var e = new CreateEvent<T>(entity);
        await HandleEventAsync(e);
        await entityStore.InsertEntityAsync(entity);
        return entity.Id;
    }

    public async Task UpdateAsync(T entity)
    {
        var e = new UpdateEvent<T>(entity);
        await HandleEventAsync(e);
        await entityStore.UpdateEntityAsync(entity);
    }

    public async Task DeleteAsync(T entity)
    {
        var e = new DeleteEvent<T>(entity);
        await HandleEventAsync(e);
        await entityStore.DeleteEntityAsync(entity);
    }

    public async Task<Guid> CreateAsync(T entity, Guid transactionId)
    {
        var e = new CreateEvent<T>(entity) { TransactionId = transactionId };
        await HandleEventAsync(e);
        await entityStore.InsertEntityAsync(entity);
        return entity.Id;
    }

    public async Task UpdateAsync(T entity, Guid transactionId)
    {
        var e = new UpdateEvent<T>(entity) { TransactionId = transactionId };
        await HandleEventAsync(e);
        await entityStore.UpdateEntityAsync(entity);
    }

    public async Task DeleteAsync(T entity, Guid transactionId)
    {
        var e = new DeleteEvent<T>(entity) { TransactionId = transactionId };
        await HandleEventAsync(e);
        await entityStore.DeleteEntityAsync(entity);
    }

    public async Task<Guid> CreateCompensationAsync(T entity, Guid transactionId)
    {
        var e = new CreateEvent<T>(entity) { TransactionId = transactionId, Compensation = true };
        await HandleEventAsync(e);
        await entityStore.InsertEntityAsync(entity);
        return entity.Id;
    }

    public async Task UpdateCompensationAsync(T entity, Guid transactionId)
    {
        var e = new UpdateEvent<T>(entity) { TransactionId = transactionId, Compensation = true };
        await HandleEventAsync(e);
        await entityStore.UpdateEntityAsync(entity);
    }

    public async Task DeleteCompensationAsync(T entity, Guid transactionId)
    {
        var e = new DeleteEvent<T>(entity) { TransactionId = transactionId, Compensation = true };
        await HandleEventAsync(e);
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

    private async Task HandleEventAsync(IEvent e)
    {
        if (globalReplayContext.IsReplaying)
        {
            switch (globalReplayContext.ReplayMode)
            {
                case ReplayMode.Strict:
                    throw new InvalidOperationException(
                        $"Cannot emit events during replay in strict mode. Event: {e.GetType().Name}"
                    );
                case ReplayMode.Sandbox:
                    globalReplayContext.AddEvent(e);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(globalReplayContext.ReplayMode));
            }
        }

        await eventStore.InsertEventAsync(e);
    }
}
