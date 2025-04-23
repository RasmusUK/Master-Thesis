using System.Collections.Concurrent;
using System.Linq.Expressions;
using EventSource.Core;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Interfaces;
using MongoDB.Driver;

namespace EventSource.Persistence.Stores;

public class MongoDbEntityStore : IEntityStore
{
    private readonly IMongoDbService mongoDbService;
    private readonly ConcurrentDictionary<Type, object> collectionCache = new();

    public MongoDbEntityStore(IMongoDbService mongoDbService)
    {
        this.mongoDbService = mongoDbService;
    }

    public async Task InsertEntityAsync<TEntity>(TEntity entity)
        where TEntity : Entity
    {
        var collection = GetCollection<TEntity>();
        await collection.InsertOneAsync(entity);
    }

    public async Task UpsertEntityAsync<TEntity>(TEntity entity)
        where TEntity : Entity
    {
        var collection = GetCollection<TEntity>();
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id);
        var updateOptions = new ReplaceOptions { IsUpsert = true };
        await collection.ReplaceOneAsync(filter, entity, updateOptions);
    }

    public Task DeleteEntityAsync<TEntity>(TEntity entity)
        where TEntity : Entity => GetCollection<TEntity>().DeleteOneAsync(e => e.Id == entity.Id);

    public async Task UpdateEntityAsync<TEntity>(TEntity entity)
        where TEntity : Entity
    {
        var collection = GetCollection<TEntity>();
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id);
        var updateOptions = new ReplaceOptions { IsUpsert = false };
        await collection.ReplaceOneAsync(filter, entity, updateOptions);
    }

    public async Task<TEntity?> GetEntityByFilterAsync<TEntity>(
        Expression<Func<TEntity, bool>> filter
    )
        where TEntity : Entity => await GetCollection<TEntity>().Find(filter).FirstOrDefaultAsync();

    public async Task<TEntity?> GetEntityByIdAsync<TEntity>(Guid id)
        where TEntity : Entity =>
        await GetCollection<TEntity>().Find(e => e.Id == id).FirstOrDefaultAsync();

    public async Task<TProjection?> GetProjectionByFilterAsync<TEntity, TProjection>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TProjection>> projection
    )
        where TEntity : Entity =>
        await GetCollection<TEntity>().Find(filter).Project(projection).FirstOrDefaultAsync();

    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync<TEntity>()
        where TEntity : Entity => await GetCollection<TEntity>().Find(_ => true).ToListAsync();

    public async Task<IReadOnlyCollection<TEntity>> GetAllByFilterAsync<TEntity>(
        Expression<Func<TEntity, bool>> filter
    )
        where TEntity : Entity => await GetCollection<TEntity>().Find(filter).ToListAsync();

    public async Task<IReadOnlyCollection<TProjection>> GetAllProjectionsAsync<
        TEntity,
        TProjection
    >(Expression<Func<TEntity, TProjection>> projection)
        where TEntity : Entity =>
        await GetCollection<TEntity>().Find(_ => true).Project(projection).ToListAsync();

    public async Task<IReadOnlyCollection<TProjection>> GetAllProjectionsByFilterAsync<
        TEntity,
        TProjection
    >(Expression<Func<TEntity, TProjection>> projection, Expression<Func<TEntity, bool>> filter)
        where TEntity : Entity =>
        await GetCollection<TEntity>().Find(filter).Project(projection).ToListAsync();

    private IMongoCollection<T> GetCollection<T>()
    {
        var type = typeof(T);

        return (IMongoCollection<T>)
            collectionCache.GetOrAdd(
                type,
                _ =>
                {
                    var collectionName = type.FullName;
                    if (string.IsNullOrEmpty(collectionName))
                        throw new InvalidOperationException("Collection name is null.");

                    return mongoDbService.GetEntityCollection<T>(collectionName);
                }
            );
    }
}
