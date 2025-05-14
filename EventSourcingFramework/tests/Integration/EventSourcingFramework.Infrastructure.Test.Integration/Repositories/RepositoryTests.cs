using System.Linq.Expressions;
using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Core.Exceptions;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Core.Models.Entity;
using EventSourcingFramework.Infrastructure.Repositories.Exceptions;
using EventSourcingFramework.Infrastructure.Repositories.Services;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Models.Events;
using EventSourcingFramework.Test.Utilities;

namespace EventSourcingFramework.Infrastructure.Test.Integration.Repositories;

[Collection("Integration")]
public class RepositoryTests : MongoIntegrationTestBase
{
    private readonly IRepository<TestEntity> repository;
    private readonly IEventStore eventStore;
    private readonly IEntityStore entityStore;
    private readonly IGlobalReplayContext replayContext;

    public RepositoryTests(
        IMongoDbService mongoDbService,
        IRepository<TestEntity> repository,
        IEventStore eventStore,
        IEntityStore entityStore,
        IGlobalReplayContext replayContext
    )
        : base(mongoDbService, replayContext)
    {
        this.repository = repository;
        this.eventStore = eventStore;
        this.entityStore = entityStore;
        this.replayContext = replayContext;
    }

    [Fact]
    public async Task CreateAsync_PersistsEntityAndEmitsMongoCreateEvent()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Created" };

        // Act
        await repository.CreateAsync(entity);
        var storedEntity = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);

        // Assert
        Assert.NotNull(storedEntity);
        Assert.Equal("Created", storedEntity!.Name);
        Assert.Single(events);
        Assert.IsType<MongoCreateEvent<TestEntity>>(events.First());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEntityAndEmitsMongoUpdateEvent()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "BeforeUpdate" };
        await repository.CreateAsync(entity);
        entity.Name = "AfterUpdate";

        // Act
        await repository.UpdateAsync(entity);
        var storedEntity = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);

        // Assert
        Assert.NotNull(storedEntity);
        Assert.Equal("AfterUpdate", storedEntity!.Name);
        Assert.Equal(2, events.Count);
        Assert.Contains(events, e => e is MongoUpdateEvent<TestEntity>);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntityAndEmitsMongoDeleteEvent()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ToBeDeleted" };
        await repository.CreateAsync(entity);

        // Act
        await repository.DeleteAsync(entity);
        var storedEntity = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);

        // Assert
        Assert.Null(storedEntity);
        Assert.Equal(2, events.Count);
        Assert.Contains(events, e => e is MongoDeleteEvent<TestEntity>);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsIfEntityNotExists()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Ghost" };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => repository.UpdateAsync(entity));
    }

    [Fact]
    public async Task CreateAsync_UsesCompensationEventOnFailure()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        var faultyRepository = new Repository<TestEntity>(
            new FailingEntityStoreWithRead(null),
            eventStore,
            replayContext
        );

        // Act & Assert
        await Assert.ThrowsAsync<RepositoryException>(() => faultyRepository.CreateAsync(entity));
        var storedEntity = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);

        // Assert
        Assert.Null(storedEntity);
        Assert.Equal(2, events.Count);
        Assert.Contains(events, e => e is MongoCreateEvent<TestEntity>);
        Assert.Contains(events, e => e is MongoDeleteEvent<TestEntity>);
    }

    [Fact]
    public async Task UpdateAsync_UsesCompensationEventOnFailure()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityBefore = new TestEntity { Id = entityId, Name = "BeforeFailure" };
        await repository.CreateAsync(entityBefore);

        var faultyRepository = new Repository<TestEntity>(
            new FailingEntityStoreWithRead(entityBefore),
            eventStore,
            replayContext
        );

        var entityAfter = new TestEntity { Id = entityId, Name = "FailsOnUpdate" };

        // Act & Assert
        await Assert.ThrowsAsync<RepositoryException>(
            () => faultyRepository.UpdateAsync(entityAfter)
        );
        var storedEntity = await entityStore.GetEntityByIdAsync<TestEntity>(entityId);
        var events = await eventStore.GetEventsByEntityIdAsync(entityId);

        // Assert
        Assert.NotNull(storedEntity);
        Assert.Equal("BeforeFailure", storedEntity!.Name);
        Assert.Equal(3, events.Count);
        Assert.Equal(
            1,
            events.Count(e => e is MongoUpdateEvent<TestEntity> ue && ue.Entity.Name == "FailsOnUpdate")
        );
        Assert.Equal(
            1,
            events.Count(e => e is MongoUpdateEvent<TestEntity> ue && ue.Entity.Name == "BeforeFailure")
        );
    }

    [Fact]
    public async Task DeleteAsync_UsesCompensationEventOnFailure()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ToDelete" };
        await repository.CreateAsync(entity);

        var faultyRepository = new Repository<TestEntity>(
            new FailingEntityStoreWithRead(null),
            eventStore,
            replayContext
        );

        // Act & Assert
        await Assert.ThrowsAsync<RepositoryException>(() => faultyRepository.DeleteAsync(entity));
        var storedEntity = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);

        // Assert
        Assert.NotNull(storedEntity);
        Assert.Equal("ToDelete", storedEntity!.Name);
        Assert.Equal(3, events.Count);
        Assert.Contains(events, e => e is MongoDeleteEvent<TestEntity>);
        Assert.Contains(events, e => e is MongoCreateEvent<TestEntity>);
    }
    
        
    [Fact]
    public async Task ReadByIdAsync_ReturnsEntity_WhenExists()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ReadById" };
        await repository.CreateAsync(entity);

        var result = await repository.ReadByIdAsync(entity.Id);

        Assert.NotNull(result);
        Assert.Equal("ReadById", result!.Name);
    }

    [Fact]
    public async Task ReadByFilterAsync_ReturnsEntity_WhenFilterMatches()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Filtered" };
        await repository.CreateAsync(entity);

        var result = await repository.ReadByFilterAsync(e => e.Name == "Filtered");

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result!.Id);
    }

    [Fact]
    public async Task ReadProjectionByIdAsync_ReturnsProjectedField()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ProjectedId" };
        await repository.CreateAsync(entity);

        var name = await repository.ReadProjectionByIdAsync(entity.Id, e => e.Name);

        Assert.Equal("ProjectedId", name);
    }

    [Fact]
    public async Task ReadProjectionByFilterAsync_ReturnsProjection_WhenMatches()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ProjectionFilter" };
        await repository.CreateAsync(entity);

        var result = await repository.ReadProjectionByFilterAsync(
            e => e.Name == "ProjectionFilter",
            e => e.Id
        );

        Assert.Equal(entity.Id, result);
    }

    [Fact]
    public async Task ReadAllAsync_ReturnsAllEntities()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "AllRead" };
        await repository.CreateAsync(entity);

        var all = await repository.ReadAllAsync();

        Assert.Contains(all, e => e.Id == entity.Id);
    }

    [Fact]
    public async Task ReadAllByFilterAsync_ReturnsMatchingEntities()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "FilteredAll" };
        await repository.CreateAsync(entity);

        var results = await repository.ReadAllByFilterAsync(e => e.Name == "FilteredAll");

        Assert.Single(results);
        Assert.Equal(entity.Id, results.First().Id);
    }

    [Fact]
    public async Task ReadAllProjectionsAsync_ReturnsAllProjectedValues()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ProjectAll" };
        await repository.CreateAsync(entity);

        var names = await repository.ReadAllProjectionsAsync(e => e.Name);

        Assert.Contains("ProjectAll", names);
    }

    [Fact]
    public async Task ReadAllProjectionsByFilterAsync_ReturnsFilteredProjections()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ProjectedFilter" };
        await repository.CreateAsync(entity);

        var names = await repository.ReadAllProjectionsByFilterAsync(
            e => e.Name,
            e => e.Name == "ProjectedFilter"
        );

        Assert.Single(names);
        Assert.Equal("ProjectedFilter", names.First());
    }

    [Fact]
    public async Task CannotEmitEventsWhileReplayingInStrict()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ShouldFail" };
        replayContext.StartReplay();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RepositoryException>(() => repository.CreateAsync(entity));
        Assert.Contains("Cannot emit events during replay", ex.Message);
    }


    private class FailingEntityStoreWithRead : IEntityStore
    {
        private readonly TestEntity snapshot;

        public FailingEntityStoreWithRead(TestEntity snapshot)
        {
            this.snapshot = snapshot;
        }

        public Task DeleteEntityAsync<T>(T entity)
            where T : IEntity => throw new RepositoryException("Delete failure");

        public Task<IReadOnlyCollection<T>> GetAllAsync<T>()
            where T : IEntity => Task.FromResult<IReadOnlyCollection<T>>(Array.Empty<T>());

        public Task<T?> GetEntityByFilterAsync<T>(Expression<Func<T, bool>> filter)
            where T : IEntity => Task.FromResult<T?>(default);

        public Task<T?> GetEntityByIdAsync<T>(Guid id)
            where T : IEntity => Task.FromResult(snapshot is T typed ? typed : default);

        public Task<IReadOnlyCollection<T>> GetAllByFilterAsync<T>(Expression<Func<T, bool>> filter)
            where T : IEntity => Task.FromResult<IReadOnlyCollection<T>>(Array.Empty<T>());

        public Task<IReadOnlyCollection<TProjection>> GetAllProjectionsAsync<T, TProjection>(
            Expression<Func<T, TProjection>> projection
        )
            where T : IEntity =>
            Task.FromResult<IReadOnlyCollection<TProjection>>(Array.Empty<TProjection>());

        public Task<IReadOnlyCollection<TProjection>> GetAllProjectionsByFilterAsync<
            T,
            TProjection
        >(Expression<Func<T, TProjection>> projection, Expression<Func<T, bool>> filter)
            where T : IEntity =>
            Task.FromResult<IReadOnlyCollection<TProjection>>(Array.Empty<TProjection>());

        public Task<TProjection?> GetProjectionByFilterAsync<T, TProjection>(
            Expression<Func<T, bool>> filter,
            Expression<Func<T, TProjection>> projection
        )
            where T : IEntity => Task.FromResult<TProjection?>(default);

        public Task InsertEntityAsync<T>(T entity)
            where T : IEntity => throw new RepositoryException("Insert failure");

        public Task UpsertEntityAsync<TEntity>(TEntity entity)
            where TEntity : IEntity => throw new RepositoryException("Upsert failure");

        public Task UpdateEntityAsync<T>(T entity)
            where T : IEntity => throw new RepositoryException("Update failure");
    }
}
