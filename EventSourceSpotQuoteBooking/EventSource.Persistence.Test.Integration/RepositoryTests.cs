using System.Linq.Expressions;
using EventSource.Application.Interfaces;
using EventSource.Core;
using EventSource.Core.Exceptions;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Events;
using EventSource.Persistence.Exceptions;
using EventSource.Persistence.Interfaces;
using EventSource.Test.Utilities;

namespace EventSource.Persistence.Test.Integration;

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
        : base(mongoDbService)
    {
        this.repository = repository;
        this.eventStore = eventStore;
        this.entityStore = entityStore;
        this.replayContext = replayContext;
    }

    [Fact]
    public async Task CreateAsync_PersistsEntityAndEmitsCreateEvent()
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
        Assert.IsType<CreateEvent<TestEntity>>(events.First());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEntityAndEmitsUpdateEvent()
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
        Assert.Contains(events, e => e is UpdateEvent<TestEntity>);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntityAndEmitsDeleteEvent()
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
        Assert.Contains(events, e => e is DeleteEvent<TestEntity>);
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
        Assert.Contains(events, e => e is CreateEvent<TestEntity>);
        Assert.Contains(events, e => e is DeleteEvent<TestEntity>);
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
            events.Count(e => e is UpdateEvent<TestEntity> ue && ue.Entity.Name == "FailsOnUpdate")
        );
        Assert.Equal(
            1,
            events.Count(e => e is UpdateEvent<TestEntity> ue && ue.Entity.Name == "BeforeFailure")
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
        Assert.Contains(events, e => e is DeleteEvent<TestEntity>);
        Assert.Contains(events, e => e is CreateEvent<TestEntity>);
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
