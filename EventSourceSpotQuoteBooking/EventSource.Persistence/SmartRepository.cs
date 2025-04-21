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

        transactionManager.TrackUpsertedEntity(entity);
        transactionManager.Enlist(
            () => inner.CreateAsync(entity, transactionManager.TransactionId),
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

        transactionManager.TrackUpsertedEntity(entity);
        transactionManager.Enlist(
            () => inner.UpdateAsync(snapshot, transactionManager.TransactionId),
            () => inner.UpdateCompensationAsync(entity, transactionManager.TransactionId)
        );
    }

    public async Task DeleteAsync(T entity)
    {
        if (!transactionManager.IsActive)
        {
            await inner.DeleteAsync(entity);
            return;
        }

        transactionManager.Enlist(
            () => inner.DeleteAsync(entity, transactionManager.TransactionId),
            () => inner.CreateCompensationAsync(entity, transactionManager.TransactionId)
        );
    }

    public async Task<T?> ReadByIdAsync(Guid id)
    {
        if (transactionManager.IsActive)
        {
            if (transactionManager.GetTrackedDeletedEntities<T>().Any(e => e.Id == id))
                return null;

            var created = transactionManager
                .GetTrackedUpsertedEntities<T>()
                .FirstOrDefault(e => e.Id == id);
            if (created != null)
                return created;
        }

        var entity = await inner.ReadByIdAsync(id);
        if (
            transactionManager.IsActive
            && entity != null
            && transactionManager.GetTrackedDeletedEntities<T>().Any(e => e.Id == entity.Id)
        )
            return null;

        return entity;
    }

    public async Task<T?> ReadByFilterAsync(Expression<Func<T, bool>> filter)
    {
        if (transactionManager.IsActive)
        {
            var compiledFilter = filter.Compile();
            var deletedIds = transactionManager
                .GetTrackedDeletedEntities<T>()
                .Select(x => x.Id)
                .ToHashSet();

            var match = transactionManager
                .GetTrackedUpsertedEntities<T>()
                .Where(x => !deletedIds.Contains(x.Id))
                .FirstOrDefault(compiledFilter);

            if (match != null)
                return match;
        }

        var result = await inner.ReadByFilterAsync(filter);
        if (
            transactionManager.IsActive
            && result != null
            && transactionManager.GetTrackedDeletedEntities<T>().Any(e => e.Id == result.Id)
        )
            return null;

        return result;
    }

    public async Task<TProjection?> ReadProjectionByIdAsync<TProjection>(
        Guid id,
        Expression<Func<T, TProjection>> projection
    )
    {
        if (transactionManager.IsActive)
        {
            if (transactionManager.GetTrackedDeletedEntities<T>().Any(e => e.Id == id))
                return default;

            var entity = transactionManager
                .GetTrackedUpsertedEntities<T>()
                .FirstOrDefault(e => e.Id == id);
            if (entity != null)
                return projection.Compile()(entity);
        }

        var dbResult = await inner.ReadProjectionByIdAsync(id, projection);
        if (
            transactionManager.IsActive
            && dbResult != null
            && transactionManager.GetTrackedDeletedEntities<T>().Any(e => e.Id == id)
        )
            return default;

        return dbResult;
    }

    public async Task<TProjection?> ReadProjectionByFilterAsync<TProjection>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, TProjection>> projection
    )
    {
        if (transactionManager.IsActive)
        {
            var compiledFilter = filter.Compile();
            var compiledProjection = projection.Compile();
            var deletedIds = transactionManager
                .GetTrackedDeletedEntities<T>()
                .Select(x => x.Id)
                .ToHashSet();

            var entity = transactionManager
                .GetTrackedUpsertedEntities<T>()
                .Where(x => !deletedIds.Contains(x.Id))
                .FirstOrDefault(compiledFilter);

            if (entity != null)
                return compiledProjection(entity);
        }

        var dbResult = await inner.ReadProjectionByFilterAsync(filter, projection);
        if (
            transactionManager.IsActive
            && dbResult != null
            && transactionManager.GetTrackedDeletedEntities<T>().Any(e => filter.Compile()(e))
        )
            return default;

        return dbResult;
    }

    public async Task<IReadOnlyCollection<T>> ReadAllAsync()
    {
        var dbResults = await inner.ReadAllAsync();
        if (!transactionManager.IsActive)
            return dbResults;

        var deletedIds = transactionManager
            .GetTrackedDeletedEntities<T>()
            .Select(x => x.Id)
            .ToHashSet();
        var inMemory = transactionManager
            .GetTrackedUpsertedEntities<T>()
            .Where(x => !deletedIds.Contains(x.Id));
        var filteredDb = dbResults.Where(x => !deletedIds.Contains(x.Id));

        return filteredDb.Concat(inMemory).ToList();
    }

    public async Task<IReadOnlyCollection<T>> ReadAllByFilterAsync(Expression<Func<T, bool>> filter)
    {
        var dbResults = await inner.ReadAllByFilterAsync(filter);
        if (!transactionManager.IsActive)
            return dbResults;

        var compiledFilter = filter.Compile();
        var deletedIds = transactionManager
            .GetTrackedDeletedEntities<T>()
            .Select(x => x.Id)
            .ToHashSet();
        var inMemoryMatches = transactionManager
            .GetTrackedUpsertedEntities<T>()
            .Where(x => !deletedIds.Contains(x.Id))
            .Where(compiledFilter);

        var filteredDb = dbResults.Where(x => !deletedIds.Contains(x.Id));

        return filteredDb.Concat(inMemoryMatches).ToList();
    }

    public async Task<IReadOnlyCollection<TProjection>> ReadAllProjectionsAsync<TProjection>(
        Expression<Func<T, TProjection>> projection
    )
    {
        var dbResults = await inner.ReadAllProjectionsAsync(projection);
        if (!transactionManager.IsActive)
            return dbResults;

        var deletedIds = transactionManager
            .GetTrackedDeletedEntities<T>()
            .Select(x => x.Id)
            .ToHashSet();
        var projected = transactionManager
            .GetTrackedUpsertedEntities<T>()
            .Where(x => !deletedIds.Contains(x.Id))
            .Select(projection.Compile());

        return dbResults.Concat(projected).ToList();
    }

    public async Task<
        IReadOnlyCollection<TProjection>
    > ReadAllProjectionsByFilterAsync<TProjection>(
        Expression<Func<T, TProjection>> projection,
        Expression<Func<T, bool>> filter
    )
    {
        var dbResults = await inner.ReadAllProjectionsByFilterAsync(projection, filter);
        if (!transactionManager.IsActive)
            return dbResults;

        var compiledFilter = filter.Compile();
        var compiledProjection = projection.Compile();
        var deletedIds = transactionManager
            .GetTrackedDeletedEntities<T>()
            .Select(x => x.Id)
            .ToHashSet();

        var projected = transactionManager
            .GetTrackedUpsertedEntities<T>()
            .Where(x => !deletedIds.Contains(x.Id))
            .Where(compiledFilter)
            .Select(compiledProjection);

        return dbResults.Concat(projected).ToList();
    }
}
