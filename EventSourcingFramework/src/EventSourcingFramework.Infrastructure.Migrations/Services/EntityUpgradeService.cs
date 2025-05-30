using EventSourcingFramework.Application.Abstractions.Migrations;
using EventSourcingFramework.Core.Models.Entity;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace EventSourcingFramework.Infrastructure.Migrations.Services;

public class EntityUpgradeService : IEntityUpgradeService
{
    private readonly IEntityCollectionNameProvider collectionNameProvider;
    private readonly IMigrationTypeRegistry migrationTypeRegistry;
    private readonly IEntityMigrator migrator;
    private readonly IMongoDbService mongoDbService;
    private readonly ISchemaVersionRegistry schemaVersionRegistry;

    public EntityUpgradeService(
        IEntityCollectionNameProvider collectionNameProvider,
        ISchemaVersionRegistry schemaVersionRegistry,
        IEntityMigrator migrator,
        IMongoDbService mongoDbService,
        IMigrationTypeRegistry migrationTypeRegistry
    )
    {
        this.collectionNameProvider = collectionNameProvider;
        this.schemaVersionRegistry = schemaVersionRegistry;
        this.migrator = migrator;
        this.mongoDbService = mongoDbService;
        this.migrationTypeRegistry = migrationTypeRegistry;
    }

    public async Task MigrateAllEntitiesToLatestVersionAsync<TEntity>()
        where TEntity : IEntity
    {
        var targetType = typeof(TEntity);
        var currentVersion = schemaVersionRegistry.GetVersion(targetType);
        var collectionName = collectionNameProvider.GetCollectionName(targetType);
        var collection = mongoDbService.GetEntityCollection<BsonDocument>(collectionName);

        var versionFilter =
            Builders<BsonDocument>.Filter.Exists("SchemaVersion", false)
            | Builders<BsonDocument>.Filter.Lt("SchemaVersion", currentVersion);

        var outdatedDocs = await collection.Find(versionFilter).ToListAsync();
        if (outdatedDocs.Count == 0)
            return;

        foreach (var doc in outdatedDocs)
        {
            var version = doc.Contains("SchemaVersion") ? doc["SchemaVersion"].AsInt32 : 1;
            var currentType = migrationTypeRegistry.GetVersionedType(targetType, version);
            var currentInstance = (IEntity)BsonSerializer.Deserialize(doc, currentType);

            while (version < currentVersion)
            {
                var nextType = migrationTypeRegistry.GetVersionedType(targetType, version + 1);
                currentInstance = migrator.Migrate(currentInstance, currentType, nextType, version);
                currentType = nextType;
                version++;
            }

            var migratedDoc = currentInstance.ToBsonDocument(targetType);
            migratedDoc["SchemaVersion"] = currentVersion;

            var filter = Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]);
            await collection.ReplaceOneAsync(filter, migratedDoc);
        }
    }

    public async Task MigrateAllEntitiesAsync()
    {
        foreach (var (type, _) in collectionNameProvider.GetAllRegistered())
        {
            var method = typeof(EntityUpgradeService)
                .GetMethod(nameof(MigrateAllEntitiesToLatestVersionAsync))!
                .MakeGenericMethod(type);

            await (Task)method.Invoke(this, null)!;
        }
    }
}