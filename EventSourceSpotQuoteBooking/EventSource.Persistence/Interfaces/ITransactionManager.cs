namespace EventSource.Persistence.Interfaces;

public interface ITransactionManager
{
    void Begin();
    bool IsActive { get; }
    Guid TransactionId { get; }

    void EnlistRollback(Func<Task> rollback);

    Task CommitAsync();
    Task RollbackAsync();
}
