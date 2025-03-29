using System.Linq.Expressions;

namespace EventSource.Core.Interfaces;

public interface IEntityStore
{
    Task UpsertEntityAsync<TEntity>(TEntity entity)
        where TEntity : Entity;

    Task DeleteEntityAsync<TEntity>(TEntity entity)
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
    Task<IReadOnlyCollection<TEntity>> GetAllAsync<TEntity>()
        where TEntity : Entity;
    Task<IReadOnlyCollection<TEntity>> GetAllByFilterAsync<TEntity>(
        Expression<Func<TEntity, bool>> filter
    )
        where TEntity : Entity;
    Task<IReadOnlyCollection<TProjection>> GetAllProjectionsAsync<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>> projection
    )
        where TEntity : Entity;
    Task<IReadOnlyCollection<TProjection>> GetAllProjectionsByFilterAsync<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        Expression<Func<TEntity, bool>> filter
    )
        where TEntity : Entity;
}
