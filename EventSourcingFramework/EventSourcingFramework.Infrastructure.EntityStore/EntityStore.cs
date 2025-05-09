using System.Linq.Expressions;
using EventSourcing.Framework.Infrastructure.Shared.Configuration.Options;
using EventSourcingFramework.Core;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Abstractions;
using EventSourcingFramework.Infrastructure.Abstractions.Migrations;
using EventSourcingFramework.Infrastructure.Abstractions.MongoDb;
using EventSourcingFramework.Infrastructure.EntityStore.Exceptions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace EventSourcingFramework.Infrastructure.EntityStore;

public class EntityStore : IEntityStore
{
    private readonly IMongoDbService mongoDbService;
    private readonly IEntityCollectionNameProvider entityCollectionNameProvider;
    private readonly ISchemaVersionRegistry schemaVersionRegistry;
    private readonly IEntityMigrator entityMigrator;
    private readonly IMigrationTypeRegistry migrationTypeRegistry;
    private readonly EventSourcingOptions eventSourcingOptions;

    public EntityStore(
        IMongoDbService mongoDbService,
        IEntityCollectionNameProvider entityCollectionNameProvider,
        IEntityMigrator entityMigrator,
        ISchemaVersionRegistry schemaVersionRegistry,
        IMigrationTypeRegistry migrationTypeRegistry,
        IOptions<EventSourcingOptions> eventSourcingOptions
    )
    {
        this.mongoDbService = mongoDbService;
        this.entityCollectionNameProvider = entityCollectionNameProvider;
        this.entityMigrator = entityMigrator;
        this.schemaVersionRegistry = schemaVersionRegistry;
        this.migrationTypeRegistry = migrationTypeRegistry;
        this.eventSourcingOptions = eventSourcingOptions.Value;
    }

    public async Task InsertEntityAsync<TEntity>(TEntity entity)
        where TEntity : IEntity
    {
        if (!eventSourcingOptions.EnableEntityStore)
            return;

        var collection = GetCollection<TEntity>();
        await collection.InsertOneAsync(entity);
    }

    public async Task UpsertEntityAsync<TEntity>(TEntity entity)
        where TEntity : IEntity
    {
        if (!eventSourcingOptions.EnableEntityStore)
            return;

        var collection = GetCollection<TEntity>();
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id);
        var updateOptions = new ReplaceOptions { IsUpsert = true };
        await collection.ReplaceOneAsync(filter, entity, updateOptions);
    }

    public async Task DeleteEntityAsync<TEntity>(TEntity entity)
        where TEntity : IEntity
    {
        if (!eventSourcingOptions.EnableEntityStore)
            return;

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
        if (!eventSourcingOptions.EnableEntityStore)
            return;

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

    public async Task<TEntity?> GetEntityByIdAsync<TEntity>(Guid id)
        where TEntity : IEntity
    {
        if (await UseMongoQuery<TEntity>())
            return await GetCollection<TEntity>().Find(e => e.Id == id).FirstOrDefaultAsync();

        var collection = GetBsonCollection(typeof(TEntity));
        var filter = Builders<BsonDocument>.Filter.Eq("_id", id);

        var doc = await collection.Find(filter).FirstOrDefaultAsync();
        return doc == null ? default : MigrateEntity<TEntity>(doc);
    }

    public async Task<TEntity?> GetEntityByFilterAsync<TEntity>(
        Expression<Func<TEntity, bool>> predicate
    )
        where TEntity : IEntity
    {
        if (await UseMongoQuery<TEntity>())
            return await GetCollection<TEntity>().Find(predicate).FirstOrDefaultAsync();

        var collection = GetBsonCollection(typeof(TEntity));
        var docs = await collection.Find(_ => true).ToListAsync();
        var compiled = predicate.Compile();

        return Enumerable.FirstOrDefault<TEntity>(docs.Select(MigrateEntity<TEntity>), compiled);
    }

    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync<TEntity>()
        where TEntity : IEntity
    {
        if (await UseMongoQuery<TEntity>())
            return await GetCollection<TEntity>().Find(_ => true).ToListAsync();

        var collection = GetBsonCollection(typeof(TEntity));
        var docs = await collection.Find(_ => true).ToListAsync();
        return Enumerable.ToList<TEntity>(docs.Select(MigrateEntity<TEntity>));
    }

    public async Task<IReadOnlyCollection<TEntity>> GetAllByFilterAsync<TEntity>(
        Expression<Func<TEntity, bool>> predicate
    )
        where TEntity : IEntity
    {
        if (await UseMongoQuery<TEntity>())
            return await GetCollection<TEntity>().Find(predicate).ToListAsync();

        var collection = GetBsonCollection(typeof(TEntity));
        var docs = await collection.Find(_ => true).ToListAsync();
        var compiled = predicate.Compile();

        return Enumerable.Where(docs.Select(MigrateEntity<TEntity>), compiled).ToList();
    }

    public async Task<TProjection?> GetProjectionByFilterAsync<TEntity, TProjection>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TProjection>> projection
    )
        where TEntity : IEntity
    {
        if (await UseMongoQuery<TEntity>())
            return await GetCollection<TEntity>()
                .Find(filter)
                .Project(projection)
                .FirstOrDefaultAsync();

        var collection = GetBsonCollection(typeof(TEntity));
        var docs = await collection.Find(_ => true).ToListAsync();
        var compiledFilter = filter.Compile();
        var compiledProjection = projection.Compile();

        return Enumerable.Where(docs.Select(MigrateEntity<TEntity>), compiledFilter)
            .Select(compiledProjection)
            .FirstOrDefault();
    }

    public async Task<IReadOnlyCollection<TProjection>> GetAllProjectionsAsync<
        TEntity,
        TProjection
    >(Expression<Func<TEntity, TProjection>> projection)
        where TEntity : IEntity
    {
        if (await UseMongoQuery<TEntity>())
            return await GetCollection<TEntity>().Find(_ => true).Project(projection).ToListAsync();

        var collection = GetBsonCollection(typeof(TEntity));
        var docs = await collection.Find(_ => true).ToListAsync();
        var compiledProjection = projection.Compile();

        return Enumerable.Select(docs.Select(MigrateEntity<TEntity>), compiledProjection).ToList();
    }

    public async Task<IReadOnlyCollection<TProjection>> GetAllProjectionsByFilterAsync<
        TEntity,
        TProjection
    >(Expression<Func<TEntity, TProjection>> projection, Expression<Func<TEntity, bool>> filter)
        where TEntity : IEntity
    {
        if (await UseMongoQuery<TEntity>())
            return await GetCollection<TEntity>().Find(filter).Project(projection).ToListAsync();

        var collection = GetBsonCollection(typeof(TEntity));
        var docs = await collection.Find(_ => true).ToListAsync();
        var compiledFilter = filter.Compile();
        var compiledProjection = projection.Compile();

        return Enumerable.Where(docs.Select(MigrateEntity<TEntity>), compiledFilter)
            .Select(compiledProjection)
            .ToList();
    }

    private TEntity MigrateEntity<TEntity>(BsonDocument doc)
        where TEntity : IEntity
    {
        var targetType = typeof(TEntity);
        var currentVersion = schemaVersionRegistry.GetVersion(targetType);
        var version = doc.Contains("SchemaVersion") ? doc["SchemaVersion"].AsInt32 : 1;

        var currentType = migrationTypeRegistry.GetVersionedType(targetType, version);
        var currentInstance = (IEntity)BsonSerializer.Deserialize(doc, currentType);

        while (version < currentVersion)
        {
            var nextType = migrationTypeRegistry.GetVersionedType(targetType, version + 1);

            currentInstance = entityMigrator.Migrate(
                currentInstance,
                currentType,
                nextType,
                version
            );
            currentType = nextType;
            version++;
        }

        return (TEntity)currentInstance;
    }

    private IMongoCollection<T> GetCollection<T>()
    {
        var collectionName = entityCollectionNameProvider.GetCollectionName(typeof(T));
        return mongoDbService.GetEntityCollection<T>(collectionName);
    }

    private IMongoCollection<BsonDocument> GetBsonCollection(Type type)
    {
        var collectionName = entityCollectionNameProvider.GetCollectionName(type);
        return mongoDbService.GetEntityCollection<BsonDocument>(collectionName);
    }

    private async Task<bool> UseMongoQuery<TEntity>()
        where TEntity : IEntity
    {
        var targetType = typeof(TEntity);
        var currentVersion = schemaVersionRegistry.GetVersion(targetType);
        var collection = GetBsonCollection(targetType);

        var versionFilter =
            Builders<BsonDocument>.Filter.Exists("SchemaVersion", false)
            | Builders<BsonDocument>.Filter.Lt("SchemaVersion", currentVersion);

        return !await collection.Find(versionFilter).AnyAsync();
    }
}
