using System.Linq.Expressions;
using EventSource.Core;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Interfaces;
using MongoDB.Driver;

namespace EventSource.Persistence.Stores;

public class MongoDbEntityStore : IEntityStore
{
    private readonly IMongoDbService mongoDbService;

    public MongoDbEntityStore(IMongoDbService mongoDbService)
    {
        this.mongoDbService = mongoDbService;
    }

    public async Task SaveEntityAsync<TEntity>(TEntity entity)
        where TEntity : Entity
    {
        var collection = GetCollection<TEntity>();
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id);
        var updateOptions = new ReplaceOptions { IsUpsert = true };
        await collection.ReplaceOneAsync(filter, entity, updateOptions);
    }

    public async Task<TEntity?> GetEntityByFilterAsync<TEntity>(
        Expression<Func<TEntity, bool>> filter
    )
        where TEntity : Entity => await GetCollection<TEntity>().Find(filter).FirstOrDefaultAsync();

    public async Task<TEntity?> GetEntityByIdAsync<TEntity>(Guid id)
        where TEntity : Entity =>
        await GetCollection<TEntity>().Find(e => e.Id == id).FirstOrDefaultAsync();

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
