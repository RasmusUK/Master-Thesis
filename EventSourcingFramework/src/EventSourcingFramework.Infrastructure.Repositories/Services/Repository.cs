using System.Linq.Expressions;
using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Application.Abstractions.Snapshots;
using EventSourcingFramework.Core.Enums;
using EventSourcingFramework.Core.Exceptions;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Core.Models.Entity;
using EventSourcingFramework.Core.Models.Events;
using EventSourcingFramework.Infrastructure.Repositories.Exceptions;
using EventSourcingFramework.Infrastructure.Shared.Models.Events;

namespace EventSourcingFramework.Infrastructure.Repositories.Services;

public class Repository<T> : IRepository<T>
    where T : IEntity
{
    private readonly IEntityStore entityStore;
    private readonly IEventStore eventStore;
    private readonly IReplayContext replayContext;
    private readonly ISnapshotService snapshotService;

    public Repository(
        IEntityStore entityStore,
        IEventStore eventStore,
        IReplayContext replayContext, ISnapshotService snapshotService)
    {
        this.entityStore = entityStore;
        this.eventStore = eventStore;
        this.replayContext = replayContext;
        this.snapshotService = snapshotService;
    }

    public async Task CreateAsync(T entity)
    {
        var eventEmitted = false;
        try
        {
            var e = new MongoCreateEvent<T>(entity);
            eventEmitted = await HandleEventAsync(e);
            await entityStore.InsertEntityAsync(entity);
            await snapshotService.TakeSnapshotIfNeededAsync(await eventStore.GetCurrentSequenceNumberAsync());
        }
        catch
        {
            if (!eventEmitted)
                throw;

            var e = new MongoDeleteEvent<T>(entity);
            eventEmitted = await HandleEventAsync(e);
            if (!eventEmitted)
                throw new RepositoryException(
                    $"Failed to emit compensation event for entity of type '{typeof(T).Name}' with id '{entity.Id}'."
                );
            throw;
        }
    }

    public async Task UpdateAsync(T entity)
    {
        var eventEmitted = false;
        var snapshot = await entityStore.GetEntityByIdAsync<T>(entity.Id);

        if (snapshot is null)
            throw new NotFoundException(
                $"Entity of type '{typeof(T).Name}' with id '{entity.Id}' not found."
            );

        try
        {
            var e = new MongoUpdateEvent<T>(entity);
            eventEmitted = await HandleEventAsync(e);
            await entityStore.UpdateEntityAsync(entity);
        }
        catch
        {
            if (!eventEmitted)
                throw;

            var e = new MongoUpdateEvent<T>(snapshot);
            eventEmitted = await HandleEventAsync(e);
            if (!eventEmitted)
                throw new RepositoryException(
                    $"Failed to emit compensation event for entity of type '{typeof(T).Name}' with id '{entity.Id}'."
                );
            throw;
        }
    }

    public async Task DeleteAsync(T entity)
    {
        var eventEmitted = false;
        try
        {
            var e = new MongoDeleteEvent<T>(entity);
            eventEmitted = await HandleEventAsync(e);
            await entityStore.DeleteEntityAsync(entity);
        }
        catch
        {
            if (!eventEmitted)
                throw;

            var e = new MongoCreateEvent<T>(entity);
            eventEmitted = await HandleEventAsync(e);
            if (!eventEmitted)
                throw new RepositoryException(
                    $"Failed to emit compensation event for entity of type '{typeof(T).Name}' with id '{entity.Id}'."
                );
            throw;
        }
    }

    public Task<T?> ReadByIdAsync(Guid id)
    {
        return entityStore.GetEntityByIdAsync<T>(id);
    }

    public Task<T?> ReadByFilterAsync(Expression<Func<T, bool>> filter)
    {
        return entityStore.GetEntityByFilterAsync(filter);
    }

    public Task<TProjection?> ReadProjectionByIdAsync<TProjection>(
        Guid id,
        Expression<Func<T, TProjection>> projection
    )
    {
        return entityStore.GetProjectionByFilterAsync(x => x.Id == id, projection);
    }

    public Task<TProjection?> ReadProjectionByFilterAsync<TProjection>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, TProjection>> projection
    )
    {
        return entityStore.GetProjectionByFilterAsync(filter, projection);
    }

    public Task<IReadOnlyCollection<T>> ReadAllAsync()
    {
        return entityStore.GetAllAsync<T>();
    }

    public Task<IReadOnlyCollection<T>> ReadAllByFilterAsync(Expression<Func<T, bool>> filter)
    {
        return entityStore.GetAllByFilterAsync(filter);
    }

    public Task<IReadOnlyCollection<TProjection>> ReadAllProjectionsAsync<TProjection>(
        Expression<Func<T, TProjection>> projection
    )
    {
        return entityStore.GetAllProjectionsAsync(projection);
    }

    public Task<IReadOnlyCollection<TProjection>> ReadAllProjectionsByFilterAsync<TProjection>(
        Expression<Func<T, TProjection>> projection,
        Expression<Func<T, bool>> filter
    )
    {
        return entityStore.GetAllProjectionsByFilterAsync(projection, filter);
    }

    public virtual async Task CreateAsync(T entity, Guid transactionId)
    {
        var e = new MongoCreateEvent<T>(entity) { TransactionId = transactionId };
        await HandleEventAsync(e);
        await entityStore.InsertEntityAsync(entity);
    }

    public virtual async Task UpdateAsync(T entity, Guid transactionId)
    {
        var e = new MongoUpdateEvent<T>(entity) { TransactionId = transactionId };
        await HandleEventAsync(e);
        await entityStore.UpdateEntityAsync(entity);
    }

    public virtual async Task DeleteAsync(T entity, Guid transactionId)
    {
        var e = new MongoDeleteEvent<T>(entity) { TransactionId = transactionId };
        await HandleEventAsync(e);
        await entityStore.DeleteEntityAsync(entity);
    }

    public async Task<Guid> CreateCompensationAsync(T entity, Guid transactionId)
    {
        var e = new MongoCreateEvent<T>(entity) { TransactionId = transactionId, Compensation = true };
        await HandleEventAsync(e);
        await entityStore.InsertEntityAsync(entity);
        return entity.Id;
    }

    public async Task UpdateCompensationAsync(T entity, Guid transactionId)
    {
        var e = new MongoUpdateEvent<T>(entity) { TransactionId = transactionId, Compensation = true };
        await HandleEventAsync(e);
        await entityStore.UpsertEntityAsync(entity);
    }

    public async Task DeleteCompensationAsync(T entity, Guid transactionId)
    {
        var e = new MongoDeleteEvent<T>(entity) { TransactionId = transactionId, Compensation = true };
        await HandleEventAsync(e);
        await entityStore.DeleteEntityAsync(entity);
    }

    private async Task<bool> HandleEventAsync(IEvent e)
    {
        if (replayContext.IsReplaying)
            switch (replayContext.ReplayMode)
            {
                case ReplayMode.Strict:
                    throw new RepositoryException(
                        $"Cannot emit events during replay in strict mode. Event: {e.GetType().Name}"
                    );
                case ReplayMode.Sandbox:
                case ReplayMode.Debug:
                case ReplayMode.Replay:
                default:
                    replayContext.AddEvent(e);
                    return true;
            }

        await eventStore.InsertEventAsync(e);
        return true;
    }
}