using System.Linq.Expressions;

namespace EventSource.Core.Interfaces;

public interface IEntityStore
{
    Task SaveEntityAsync<TEntity>(TEntity entity)
        where TEntity : Entity;

    Task<TEntity?> GetEntityByFilterAsync<TEntity>(Expression<Func<TEntity, bool>> filter)
        where TEntity : Entity;
    Task<TEntity?> GetEntityByIdAsync<TEntity>(Guid id)
        where TEntity : Entity;
    Task<TProjection?> GetProjectionByFilterAsync<TEntity, TProjection>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TProjection>> projection
    )
        where TEntity : Entity;
}
