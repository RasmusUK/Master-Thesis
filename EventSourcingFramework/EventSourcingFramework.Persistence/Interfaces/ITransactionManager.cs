using EventSourcingFramework.Core;

namespace EventSourcingFramework.Persistence.Interfaces;

public interface ITransactionManager
{
    Guid TransactionId { get; }
    bool IsActive { get; }

    void Begin();
    void Enlist(Func<Task> commit, Func<Task> rollback);
    Task CommitAsync();
    Task RollbackAsync();
    IEnumerable<T> GetTrackedUpsertedEntities<T>()
        where T : IEntity;

    void TrackUpsertedEntity<T>(T entity)
        where T : IEntity;
    IEnumerable<T> GetTrackedDeletedEntities<T>()
        where T : IEntity;

    void TrackDeletedEntity<T>(T entity)
        where T : IEntity;
}
