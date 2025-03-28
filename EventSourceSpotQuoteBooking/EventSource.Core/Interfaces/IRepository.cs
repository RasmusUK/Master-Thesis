using System.Linq.Expressions;

namespace EventSource.Core.Interfaces;

public interface IRepository<T>
    where T : Entity
{
    Task<Guid> CreateAsync(T entity);
    Task<T?> ReadByIdAsync(Guid id);
    Task<T?> ReadByFilterAsync(Expression<Func<T, bool>> filter);
    Task<T?> ReadProjectionByFilterAsync<TProjection>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, TProjection>> projection
    );
    Task<IReadOnlyCollection<T>> ReadAllAsync();
    Task<IReadOnlyCollection<T>> ReadAllByFilterAsync(Expression<Func<T, bool>> filter);
    Task<IReadOnlyCollection<TProjection>> ReadAllProjectionsAsync<TProjection>(
        Expression<Func<T, TProjection>> projection
    );
    Task<IReadOnlyCollection<TProjection>> ReadAllProjectionsByFilterAsync<TProjection>(
        Expression<Func<T, TProjection>> projection,
        Expression<Func<T, bool>> filter
    );
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}
