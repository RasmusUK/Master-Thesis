using System.Linq.Expressions;
using EventSource.Core;
using EventSource.Core.Exceptions;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Interfaces;

namespace EventSource.Persistence;

public class SmartRepository<T> : IRepository<T>
    where T : Entity
{
    private readonly Repository<T> inner;
    private readonly ITransactionManager transactionManager;

    public SmartRepository(Repository<T> inner, ITransactionManager transactionManager)
    {
        this.inner = inner;
        this.transactionManager = transactionManager;
    }

    public async Task<Guid> CreateAsync(T entity)
    {
        if (!transactionManager.IsActive)
            return await inner.CreateAsync(entity);

        await inner.CreateAsync(entity, transactionManager.TransactionId);
        transactionManager.EnlistRollback(
            () => inner.DeleteCompensationAsync(entity, transactionManager.TransactionId)
        );

        return entity.Id;
    }

    public async Task UpdateAsync(T entity)
    {
        if (!transactionManager.IsActive)
        {
            await inner.UpdateAsync(entity);
            return;
        }

        var snapshot = await inner.ReadByIdAsync(entity.Id);
        if (snapshot is null)
            throw new NotFoundException(
                $"Entity of type '{typeof(T).Name}' with id '{entity.Id}' not found."
            );

        await inner.UpdateAsync(entity, transactionManager.TransactionId);
        transactionManager.EnlistRollback(
            () => inner.UpdateCompensationAsync(snapshot, transactionManager.TransactionId)
        );
    }

    public async Task DeleteAsync(T entity)
    {
        if (!transactionManager.IsActive)
        {
            await inner.DeleteAsync(entity);
            return;
        }

        await inner.DeleteAsync(entity, transactionManager.TransactionId);
        transactionManager.EnlistRollback(
            () => inner.CreateCompensationAsync(entity, transactionManager.TransactionId)
        );
    }

    public Task<T?> ReadByIdAsync(Guid id) => inner.ReadByIdAsync(id);

    public Task<T?> ReadByFilterAsync(Expression<Func<T, bool>> filter) =>
        inner.ReadByFilterAsync(filter);

    public Task<TProjection?> ReadProjectionByIdAsync<TProjection>(
        Guid id,
        Expression<Func<T, TProjection>> projection
    ) => inner.ReadProjectionByIdAsync(id, projection);

    public Task<TProjection?> ReadProjectionByFilterAsync<TProjection>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, TProjection>> projection
    ) => inner.ReadProjectionByFilterAsync(filter, projection);

    public Task<IReadOnlyCollection<T>> ReadAllAsync() => inner.ReadAllAsync();

    public Task<IReadOnlyCollection<T>> ReadAllByFilterAsync(Expression<Func<T, bool>> filter) =>
        inner.ReadAllByFilterAsync(filter);

    public Task<IReadOnlyCollection<TProjection>> ReadAllProjectionsAsync<TProjection>(
        Expression<Func<T, TProjection>> projection
    ) => inner.ReadAllProjectionsAsync(projection);

    public Task<IReadOnlyCollection<TProjection>> ReadAllProjectionsByFilterAsync<TProjection>(
        Expression<Func<T, TProjection>> projection,
        Expression<Func<T, bool>> filter
    ) => inner.ReadAllProjectionsByFilterAsync(projection, filter);
}
