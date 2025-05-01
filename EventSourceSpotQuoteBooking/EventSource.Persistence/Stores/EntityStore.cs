using System.Linq.Expressions;
using EventSource.Core;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Exceptions;
using EventSource.Persistence.Interfaces;
using MongoDB.Driver;

namespace EventSource.Persistence.Stores;

public class EntityStore : IEntityStore
{
    private readonly IMongoDbService mongoDbService;
    private readonly IEntityCollectionNameProvider entityCollectionNameProvider;

    public EntityStore(
        IMongoDbService mongoDbService,
        IEntityCollectionNameProvider entityCollectionNameProvider
    )
    {
        this.mongoDbService = mongoDbService;
        this.entityCollectionNameProvider = entityCollectionNameProvider;
    }

    public async Task InsertEntityAsync<TEntity>(TEntity entity)
        where TEntity : IEntity
    {
        var collection = GetCollection<TEntity>();
        await collection.InsertOneAsync(entity);
    }

    public async Task UpsertEntityAsync<TEntity>(TEntity entity)
        where TEntity : IEntity
    {
        var collection = GetCollection<TEntity>();

        var existing = await collection.Find(e => e.Id == entity.Id).FirstOrDefaultAsync();

        if (existing == null)
        {
            entity.ConcurrencyVersion = 1;
            await collection.InsertOneAsync(entity);
        }
        else
        {
            if (existing.ConcurrencyVersion != entity.ConcurrencyVersion)
                throw new EntityStoreException("Upsert failed due to version mismatch.");

            entity.ConcurrencyVersion++;

            var filter = Builders<TEntity>.Filter.And(
                Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id),
                Builders<TEntity>.Filter.Eq(e => e.ConcurrencyVersion, existing.ConcurrencyVersion)
            );

            var result = await collection.ReplaceOneAsync(filter, entity);

            if (result.MatchedCount == 0)
                throw new EntityStoreException("Upsert failed due to concurrency violation.");
        }
    }

    public async Task DeleteEntityAsync<TEntity>(TEntity entity)
        where TEntity : IEntity
    {
        var collection = GetCollection<TEntity>();

        var filter = Builders<TEntity>.Filter.And(
            Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id),
            Builders<TEntity>.Filter.Eq(e => e.ConcurrencyVersion, entity.ConcurrencyVersion)
        );

        var result = await collection.DeleteOneAsync(filter);
        if (result.DeletedCount == 0)
            throw new EntityStoreException(
                "Delete failed due to concurrency violation or entity not existing."
            );
    }

    public async Task UpdateEntityAsync<TEntity>(TEntity entity)
        where TEntity : IEntity
    {
        var collection = GetCollection<TEntity>();

        var filter = Builders<TEntity>.Filter.And(
            Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id),
            Builders<TEntity>.Filter.Eq(e => e.ConcurrencyVersion, entity.ConcurrencyVersion)
        );

        entity.ConcurrencyVersion++;

        var updateOptions = new ReplaceOptions { IsUpsert = false };

        var result = await collection.ReplaceOneAsync(filter, entity, updateOptions);

        if (result.MatchedCount == 0)
            throw new EntityStoreException(
                "Update failed due to concurrency violation or entity not existing."
            );
    }

    public async Task<TEntity?> GetEntityByFilterAsync<TEntity>(
        Expression<Func<TEntity, bool>> filter
    )
        where TEntity : IEntity =>
        await GetCollection<TEntity>().Find(filter).FirstOrDefaultAsync();

    public async Task<TEntity?> GetEntityByIdAsync<TEntity>(Guid id)
        where TEntity : IEntity =>
        await GetCollection<TEntity>().Find(e => e.Id == id).FirstOrDefaultAsync();

    public async Task<TProjection?> GetProjectionByFilterAsync<TEntity, TProjection>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TProjection>> projection
    )
        where TEntity : IEntity =>
        await GetCollection<TEntity>().Find(filter).Project(projection).FirstOrDefaultAsync();

    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync<TEntity>()
        where TEntity : IEntity => await GetCollection<TEntity>().Find(_ => true).ToListAsync();

    public async Task<IReadOnlyCollection<TEntity>> GetAllByFilterAsync<TEntity>(
        Expression<Func<TEntity, bool>> filter
    )
        where TEntity : IEntity => await GetCollection<TEntity>().Find(filter).ToListAsync();

    public async Task<IReadOnlyCollection<TProjection>> GetAllProjectionsAsync<
        TEntity,
        TProjection
    >(Expression<Func<TEntity, TProjection>> projection)
        where TEntity : IEntity =>
        await GetCollection<TEntity>().Find(_ => true).Project(projection).ToListAsync();

    public async Task<IReadOnlyCollection<TProjection>> GetAllProjectionsByFilterAsync<
        TEntity,
        TProjection
    >(Expression<Func<TEntity, TProjection>> projection, Expression<Func<TEntity, bool>> filter)
        where TEntity : IEntity =>
        await GetCollection<TEntity>().Find(filter).Project(projection).ToListAsync();

    private IMongoCollection<T> GetCollection<T>()
    {
        var collectionName = entityCollectionNameProvider.GetCollectionName(typeof(T));
        return mongoDbService.GetEntityCollection<T>(collectionName);
    }
}
