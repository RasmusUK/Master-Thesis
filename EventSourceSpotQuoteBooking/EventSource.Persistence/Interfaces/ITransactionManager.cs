using EventSource.Core;

namespace EventSource.Persistence.Interfaces;

public interface ITransactionManager
{
    Guid TransactionId { get; }
    bool IsActive { get; }

    void Begin();
    void Enlist(Func<Task> commit, Func<Task> rollback);
    Task CommitAsync();
    Task RollbackAsync();
    IEnumerable<T> GetTrackedUpsertedEntities<T>()
        where T : Entity;

    void TrackUpsertedEntity<T>(T entity)
        where T : Entity;
    IEnumerable<T> GetTrackedDeletedEntities<T>()
        where T : Entity;

    void TrackDeletedEntity<T>(T entity)
        where T : Entity;
}
