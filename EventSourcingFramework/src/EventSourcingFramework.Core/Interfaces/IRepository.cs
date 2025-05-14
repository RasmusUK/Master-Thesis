using System.Linq.Expressions;
using EventSourcingFramework.Core.Models.Entity;

namespace EventSourcingFramework.Core.Interfaces;

public interface IRepository<T>
    where T : IEntity
{
    Task CreateAsync(T entity);
    Task<T?> ReadByIdAsync(Guid id);
    Task<T?> ReadByFilterAsync(Expression<Func<T, bool>> filter);

    Task<TProjection?> ReadProjectionByIdAsync<TProjection>(
        Guid id,
        Expression<Func<T, TProjection>> projection
    );

    Task<TProjection?> ReadProjectionByFilterAsync<TProjection>(
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
    Task DeleteAsync(T entity);
}