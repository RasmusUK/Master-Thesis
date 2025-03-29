using System.Linq.Expressions;
using EventSource.Core;
using EventSource.Persistence.Interfaces;
using MongoDB.Driver;

namespace EventSource.Persistence.Stores;

public class MongoDbEntityStore : IMongoDbEntityStore
{
    private readonly IMongoDbService mongoDbService;

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

    public async Task InsertEntityAsync<TEntity>(TEntity entity, IClientSessionHandle session)
        where TEntity : Entity
    {
        var collection = GetCollection<TEntity>();
        await collection.InsertOneAsync(session, entity);
    }

    public async Task UpdateEntityAsync<TEntity>(TEntity entity)
        where TEntity : Entity
    {
        var collection = GetCollection<TEntity>();
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id);
        var updateOptions = new ReplaceOptions { IsUpsert = false };
        await collection.ReplaceOneAsync(filter, entity, updateOptions);
    }

    public async Task UpdateEntityAsync<TEntity>(TEntity entity, IClientSessionHandle session)
        where TEntity : Entity
    {
        var collection = GetCollection<TEntity>();
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id);
        var updateOptions = new ReplaceOptions { IsUpsert = false };
        await collection.ReplaceOneAsync(session, filter, entity, updateOptions);
    }

    public Task DeleteEntityAsync<TEntity>(TEntity entity, IClientSessionHandle session)
        where TEntity : Entity =>
        GetCollection<TEntity>().DeleteOneAsync(session, e => e.Id == entity.Id);

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

    private IMongoCollection<TEntity> GetCollection<TEntity>()
        where TEntity : Entity
    {
        var collectionName = typeof(TEntity).FullName;
        if (string.IsNullOrEmpty(collectionName))
            throw new InvalidOperationException("Collection name is null.");
        return mongoDbService.GetCollection<TEntity>(collectionName);
        ;
    }
}
