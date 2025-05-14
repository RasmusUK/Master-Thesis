using EventSourcing.Framework.Infrastructure.Shared.Configuration.Options;
using EventSourcing.Framework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Application.Abstractions;
using EventSourcingFramework.Application.Abstractions.Migrations;
using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Stores.EntityStore;
using EventSourcingFramework.Infrastructure.Stores.EntityStore.Exceptions;
using EventSourcingFramework.Test.Utilities;
using MongoDB.Bson;

namespace EventSourcingFramework.Infrastructure.Test.Integration;

[Collection("Integration")]
public class EntityStoreTests : MongoIntegrationTestBase
{
    private readonly IEntityStore store;
    private readonly IMongoDbService mongoDbService;
    private readonly IEntityCollectionNameProvider nameProvider;
    private readonly IEntityMigrator entityMigrator;
    private readonly ISchemaVersionRegistry schemaVersionRegistry;
    private readonly IMigrationTypeRegistry migrationTypeRegistry;

    public EntityStoreTests(
        IMongoDbService mongoDbService,
        IGlobalReplayContext replayContext,
        IEntityStore store,
        IEntityCollectionNameProvider nameProvider, IEntityMigrator entityMigrator, ISchemaVersionRegistry schemaVersionRegistry, IMigrationTypeRegistry migrationTypeRegistry)
        : base(mongoDbService, replayContext)
    {
        this.store = store;
        this.nameProvider = nameProvider;
        this.entityMigrator = entityMigrator;
        this.schemaVersionRegistry = schemaVersionRegistry;
        this.migrationTypeRegistry = migrationTypeRegistry;
        this.mongoDbService = mongoDbService;
    }

    [Fact]
    public async Task InsertEntityAsync_PersistsEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "InsertMe" };

        // Act
        await store.InsertEntityAsync(entity);
        var loaded = await store.GetEntityByIdAsync<TestEntity>(entity.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("InsertMe", loaded!.Name);
    }

    [Fact]
    public async Task UpsertEntityAsync_InsertsOrUpdatesEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "UpsertMe" };

        // Act
        await store.UpsertEntityAsync(entity);
        var inserted = await store.GetEntityByIdAsync<TestEntity>(entity.Id);

        entity.Name = "UpdatedName";
        await store.UpsertEntityAsync(entity);
        var updated = await store.GetEntityByIdAsync<TestEntity>(entity.Id);

        // Assert
        Assert.NotNull(inserted);
        Assert.Equal("UpdatedName", updated!.Name);
    }

    [Fact]
    public async Task UpdateEntityAsync_UsesOptimisticConcurrency()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "BeforeUpdate",
            ConcurrencyVersion = 0,
        };
        await store.InsertEntityAsync(entity);

        // Act
        entity.Name = "AfterUpdate";
        await store.UpdateEntityAsync(entity);
        var result = await store.GetEntityByIdAsync<TestEntity>(entity.Id);

        // Assert
        Assert.Equal("AfterUpdate", result!.Name);
        Assert.Equal(1, result.ConcurrencyVersion);
    }

    [Fact]
    public async Task UpdateEntityAsync_ThrowsOnConcurrencyViolation()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Conflicted",
            ConcurrencyVersion = 0,
        };
        await store.InsertEntityAsync(entity);

        var stale = new TestEntity
        {
            Id = entity.Id,
            Name = "Stale",
            ConcurrencyVersion = 0,
        };

        // Act & Assert
        await store.UpdateEntityAsync(entity); // bumps version to 1
        await Assert.ThrowsAsync<EntityStoreException>(() => store.UpdateEntityAsync(stale));
    }

    [Fact]
    public async Task DeleteEntityAsync_DeletesCorrectly()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "DeleteMe",
            ConcurrencyVersion = 0,
        };
        await store.InsertEntityAsync(entity);

        // Act
        await store.DeleteEntityAsync(entity);
        var deleted = await store.GetEntityByIdAsync<TestEntity>(entity.Id);

        // Assert
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteEntityAsync_ThrowsOnConcurrencyViolation()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "DeleteFail",
            ConcurrencyVersion = 0,
        };
        await store.InsertEntityAsync(entity);

        // Act & Assert
        entity.ConcurrencyVersion = 1;
        await Assert.ThrowsAsync<EntityStoreException>(() => store.DeleteEntityAsync(entity));
    }

    [Fact]
    public async Task GetEntityByFilterAsync_ReturnsMatching()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Filtered" };
        await store.InsertEntityAsync(entity);

        // Act
        var result = await store.GetEntityByFilterAsync<TestEntity>(e => e.Name == "Filtered");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result!.Id);
    }

    [Fact]
    public async Task GetProjectionByFilterAsync_ReturnsProjectedValue()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ProjectMe" };
        await store.InsertEntityAsync(entity);

        // Act
        var nameOnly = await store.GetProjectionByFilterAsync<TestEntity, string>(
            e => e.Name == "ProjectMe",
            e => e.Name
        );

        // Assert
        Assert.Equal("ProjectMe", nameOnly);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        // Arrange
        var e1 = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        var e2 = new TestEntity { Id = Guid.NewGuid(), Name = "B" };
        await store.InsertEntityAsync(e1);
        await store.InsertEntityAsync(e2);

        // Act
        var all = await store.GetAllAsync<TestEntity>();

        // Assert
        Assert.Contains(all, e => e.Id == e1.Id);
        Assert.Contains(all, e => e.Id == e2.Id);
    }

    [Fact]
    public async Task GetAllByFilterAsync_ReturnsFiltered()
    {
        // Arrange
        var e1 = new TestEntity { Id = Guid.NewGuid(), Name = "Match" };
        var e2 = new TestEntity { Id = Guid.NewGuid(), Name = "Other" };
        await store.InsertEntityAsync(e1);
        await store.InsertEntityAsync(e2);

        // Act
        var filtered = await store.GetAllByFilterAsync<TestEntity>(e => e.Name == "Match");

        // Assert
        Assert.Single(filtered);
        Assert.Equal("Match", filtered.First().Name);
    }

    [Fact]
    public async Task GetAllProjectionsByFilterAsync_ReturnsProjectedFilteredValues()
    {
        // Arrange
        var e1 = new TestEntity { Id = Guid.NewGuid(), Name = "Good" };
        var e2 = new TestEntity { Id = Guid.NewGuid(), Name = "Bad" };
        await store.InsertEntityAsync(e1);
        await store.InsertEntityAsync(e2);

        // Act
        var projected = await store.GetAllProjectionsByFilterAsync<TestEntity, string>(
            e => e.Name,
            e => e.Name == "Good"
        );

        // Assert
        Assert.Single(projected);
        Assert.Contains("Good", projected);
        Assert.DoesNotContain("Bad", projected);
    }

    [Fact]
    public async Task GetEntityByIdAsync_DeserializesOldVersion_AndMigrates()
    {
        // Arrange
        var old = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Doe",
        };
        var doc = old.ToBsonDocument();
        var collection = mongoDbService.GetEntityCollection<BsonDocument>(
            nameProvider.GetCollectionName(typeof(TestEntity))
        );
        await collection.InsertOneAsync(doc);

        // Act
        var migrated = await store.GetEntityByIdAsync<TestEntity>(old.Id);

        // Assert
        Assert.NotNull(migrated);
        Assert.Equal("Jane - Doe", migrated.Name);
        Assert.Equal(3, migrated.SchemaVersion);
    }

    [Fact]
    public async Task GetEntityByFilterAsync_MigratesEntityBeforeFilter()
    {
        // Arrange
        var old = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "Filter",
            LastName = "Check",
        };
        var doc = old.ToBsonDocument();
        var collection = mongoDbService.GetEntityCollection<BsonDocument>(
            nameProvider.GetCollectionName(typeof(TestEntity))
        );
        await collection.InsertOneAsync(doc);

        // Act
        var found = await store.GetEntityByFilterAsync<TestEntity>(x => x.Name.Contains("Check"));

        // Assert
        Assert.NotNull(found);
        Assert.Equal(old.Id, found!.Id);
    }

    [Fact]
    public async Task GetAllAsync_MigratesAllEntities()
    {
        // Arrange
        var e1 = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "All",
            LastName = "Old",
        };
        var e2 = new TestEntity { Id = Guid.NewGuid(), Name = "All New" };

        var collection = mongoDbService.GetEntityCollection<BsonDocument>(
            nameProvider.GetCollectionName(typeof(TestEntity))
        );

        await collection.InsertOneAsync(e1.ToBsonDocument());
        await collection.InsertOneAsync(e2.ToBsonDocument());

        // Act
        var all = await store.GetAllAsync<TestEntity>();

        // Assert
        Assert.Contains(all, e => e.Id == e1.Id && e.Name == "All - Old");
        Assert.Contains(all, e => e.Id == e2.Id && e.Name == "All New");
    }

    [Fact]
    public async Task GetAllByFilterAsync_FiltersMigratedEntities()
    {
        // Arrange
        var v1 = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "Filter",
            LastName = "Match",
        };
        var v2 = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "Filter",
            LastName = "Ignore",
        };

        var collection = mongoDbService.GetEntityCollection<BsonDocument>(
            nameProvider.GetCollectionName(typeof(TestEntity))
        );
        await collection.InsertManyAsync(new[] { v1.ToBsonDocument(), v2.ToBsonDocument() });

        // Act
        var results = await store.GetAllByFilterAsync<TestEntity>(e => e.Name == "Filter - Match");

        // Assert
        Assert.Single(results);
        Assert.Equal("Filter - Match", results.First().Name);
    }
    
    [Fact]
    public async Task GetAllProjectionsAsync_MigratesAndProjectsEntities()
    {
        // Arrange
        var v1 = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "Project",
            LastName = "Me"
        };
        var v2 = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "Skip",
            LastName = "This"
        };

        var collection = mongoDbService.GetEntityCollection<BsonDocument>(
            nameProvider.GetCollectionName(typeof(TestEntity))
        );
        await collection.InsertManyAsync(new[] { v1.ToBsonDocument(), v2.ToBsonDocument() });

        // Act
        var projections = await store.GetAllProjectionsAsync<TestEntity, string>(e => e.Name);

        // Assert
        Assert.Contains("Project - Me", projections);
        Assert.Contains("Skip - This", projections);
        Assert.Equal(2, projections.Count);
    }

    [Fact]
    public async Task GetAllProjectionsByFilterAsync_MigratesAndFiltersAndProjects()
    {
        // Arrange
        var v1 = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "Project",
            LastName = "Keep"
        };
        var v2 = new TestEntity1
        {
            Id = Guid.NewGuid(),
            FirstName = "Project",
            LastName = "Drop"
        };

        var collection = mongoDbService.GetEntityCollection<BsonDocument>(
            nameProvider.GetCollectionName(typeof(TestEntity))
        );
        await collection.InsertManyAsync(new[] { v1.ToBsonDocument(), v2.ToBsonDocument() });

        // Act
        var projections = await store.GetAllProjectionsByFilterAsync<TestEntity, string>(
            e => e.Name,
            e => e.Name == "Project - Keep"
        );

        // Assert
        Assert.Single(projections);
        Assert.Equal("Project - Keep", projections.First());
    }

    
    [Fact]
    public async Task InsertEntityAsync_DoesNothing_WhenEntityStoreIsDisabled()
    {
        // Arrange
        var entityStore = new EntityStore(
            MongoDbService, nameProvider, entityMigrator, schemaVersionRegistry, migrationTypeRegistry,
            Microsoft.Extensions.Options.Options.Create(new EventSourcingOptions
            {
                EnableEntityStore = false
            }));

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "DisabledStore" };

        // Act
        await entityStore.InsertEntityAsync(entity);
        var entities = await entityStore.GetAllAsync<TestEntity>();

        // Assert
        Assert.Empty(entities);
    }
}
