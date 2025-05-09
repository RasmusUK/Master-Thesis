using System.Linq.Expressions;

namespace EventSourcingFramework.Core.Interfaces;

public interface IEntityStore
{
    Task InsertEntityAsync<TEntity>(TEntity IEntity)
        where TEntity : IEntity;
    Task UpsertEntityAsync<TEntity>(TEntity IEntity)
        where TEntity : IEntity;
    Task UpdateEntityAsync<TEntity>(TEntity IEntity)
        where TEntity : IEntity;
    Task DeleteEntityAsync<TEntity>(TEntity IEntity)
        where TEntity : IEntity;
    Task<TEntity?> GetEntityByFilterAsync<TEntity>(Expression<Func<TEntity, bool>> filter)
        where TEntity : IEntity;
    Task<TEntity?> GetEntityByIdAsync<TEntity>(Guid id)
        where TEntity : IEntity;
    Task<TProjection?> GetProjectionByFilterAsync<TEntity, TProjection>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TProjection>> projection
    )
        where TEntity : IEntity;
    Task<IReadOnlyCollection<TEntity>> GetAllAsync<TEntity>()
        where TEntity : IEntity;
    Task<IReadOnlyCollection<TEntity>> GetAllByFilterAsync<TEntity>(
        Expression<Func<TEntity, bool>> filter
    )
        where TEntity : IEntity;
    Task<IReadOnlyCollection<TProjection>> GetAllProjectionsAsync<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>> projection
    )
        where TEntity : IEntity;
    Task<IReadOnlyCollection<TProjection>> GetAllProjectionsByFilterAsync<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        Expression<Func<TEntity, bool>> filter
    )
        where TEntity : IEntity;
}
