using System.Transactions;
using EventSource.Core;
using EventSource.Persistence.Interfaces;

namespace EventSource.Persistence;

public class TransactionManager : ITransactionManager
{
    private Queue<(Func<Task> commit, Func<Task> rollback)>? actions;
    public bool IsActive => actions is not null;
    public Guid TransactionId { get; init; } = Guid.NewGuid();
    private Queue<Func<Task>>? rollbackActions;
    private Dictionary<Type, List<object>>? trackedUpsertedEntities;
    private Dictionary<Type, List<object>>? trackedDeletedEntities;

    public void Begin()
    {
        if (IsActive)
            throw new TransactionException("Transaction already started.");

        actions = new Queue<(Func<Task>, Func<Task>)>();
        rollbackActions = new Queue<Func<Task>>();
        trackedUpsertedEntities = new Dictionary<Type, List<object>>();
        trackedDeletedEntities = new Dictionary<Type, List<object>>();
    }

    public void TrackUpsertedEntity<T>(T entity)
        where T : Entity
    {
        if (!IsActive)
            throw new TransactionException("No active transaction.");

        if (!trackedUpsertedEntities!.TryGetValue(typeof(T), out var list))
        {
            list = new List<object>();
            trackedUpsertedEntities.Add(typeof(T), list);
        }

        list.Add(entity);
    }

    public void TrackDeletedEntity<T>(T entity)
        where T : Entity
    {
        if (!IsActive)
            throw new TransactionException("No active transaction.");

        if (!trackedDeletedEntities!.TryGetValue(typeof(T), out var list))
        {
            list = new List<object>();
            trackedDeletedEntities.Add(typeof(T), list);
        }

        list.Add(entity);
    }

    public void Enlist(Func<Task> commit, Func<Task> rollback)
    {
        if (!IsActive)
            throw new TransactionException("No active transaction.");

        actions!.Enqueue((commit, rollback));
    }

    public Task CommitAsync()
    {
        if (!IsActive)
            throw new TransactionException("No active transaction.");

        while (actions!.Count > 0)
        {
            var (commit, rollback) = actions.Dequeue();
            rollbackActions!.Enqueue(rollback);
            commit();
        }

        Clear();
        return Task.CompletedTask;
    }

    public async Task RollbackAsync()
    {
        if (!IsActive)
            throw new TransactionException("No active transaction.");

        while (rollbackActions!.Count > 0)
        {
            var rollback = rollbackActions.Dequeue();
            try
            {
                await rollback();
            }
            catch
            {
                throw new TransactionException("Rollback failed.");
            }
        }

        Clear();
    }

    public IEnumerable<T> GetTrackedUpsertedEntities<T>()
        where T : Entity
    {
        if (!IsActive)
            return Enumerable.Empty<T>();

        return trackedUpsertedEntities!.TryGetValue(typeof(T), out var list)
            ? list.Cast<T>()
            : Enumerable.Empty<T>();
    }

    public IEnumerable<T> GetTrackedDeletedEntities<T>()
        where T : Entity
    {
        if (!IsActive)
            return Enumerable.Empty<T>();

        return trackedDeletedEntities!.TryGetValue(typeof(T), out var list)
            ? list.Cast<T>()
            : Enumerable.Empty<T>();
    }

    private void Clear()
    {
        actions = null;
        rollbackActions = null;
        trackedUpsertedEntities = null;
        trackedDeletedEntities = null;
    }
}
