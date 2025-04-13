using System.Transactions;
using EventSource.Persistence.Interfaces;

namespace EventSource.Persistence;

public class TransactionManager : ITransactionManager
{
    private Stack<Func<Task>>? rollbackActions;
    public bool IsActive => rollbackActions is not null;

    public void Begin()
    {
        if (IsActive)
            throw new TransactionException("Transaction already started.");

        rollbackActions = new Stack<Func<Task>>();
    }

    public void EnlistRollback(Func<Task> rollback)
    {
        if (!IsActive)
            throw new Exceptions.TransactionException("No active transaction.");

        rollbackActions!.Push(rollback);
    }

    public Task CommitAsync()
    {
        Clear();
        return Task.CompletedTask;
    }

    public async Task RollbackAsync()
    {
        if (!IsActive)
            return;

        while (rollbackActions!.Count > 0)
        {
            var rollback = rollbackActions.Pop();
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

    private void Clear()
    {
        rollbackActions = null;
    }
}
