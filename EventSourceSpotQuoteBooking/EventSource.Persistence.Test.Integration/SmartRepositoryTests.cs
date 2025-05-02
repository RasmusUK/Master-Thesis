using EventSource.Application;
using EventSource.Application.Interfaces;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Events;
using EventSource.Persistence.Interfaces;
using EventSource.Test.Utilities;

namespace EventSource.Persistence.Test.Integration;

[Collection("Integration")]
public class SmartRepositoryTests : MongoIntegrationTestBase
{
    private readonly Repository<TestEntity> baseRepository;
    private readonly IEventStore eventStore;
    private readonly IEntityStore entityStore;

    public SmartRepositoryTests(
        IMongoDbService mongoDbService,
        Repository<TestEntity> baseRepository,
        IEventStore eventStore,
        IEntityStore entityStore
    )
        : base(mongoDbService)
    {
        this.baseRepository = baseRepository;
        this.eventStore = eventStore;
        this.entityStore = entityStore;
    }

    [Fact]
    public async Task CreateAsync_WithinTransaction_CommitsEventAndEntity()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(baseRepository, transactionManager);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "CreatedTxn" };
        transactionManager.Begin();

        // Act
        await smartRepo.CreateAsync(entity);
        await transactionManager.CommitAsync();
        var stored = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);

        // Assert
        Assert.NotNull(stored);
        Assert.Single(events);
        Assert.IsType<CreateEvent<TestEntity>>(events.First());
    }

    [Fact]
    public async Task CreateSucceeds_UpdateFails_TriggersCompensationDelete()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "TestEntity" };

        var repo = new FailingRepository(
            entityStore,
            eventStore,
            new GlobalReplayContext(),
            failOnMethod: nameof(Repository<TestEntity>.UpdateAsync)
        );

        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(repo, transactionManager);

        transactionManager.Begin();

        await smartRepo.CreateAsync(entity); // enlist create (succeeds)
        await smartRepo.UpdateAsync(entity); // enlist update (will fail on commit)

        // Act
        await Assert.ThrowsAsync<Exception>(() => transactionManager.CommitAsync());
        await transactionManager.RollbackAsync();

        // Assert
        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);

        Assert.Equal(2, events.Count); // Create + Compensation Delete
        Assert.Contains(events, e => e is CreateEvent<TestEntity>);
        Assert.Contains(events, e => e is DeleteEvent<TestEntity> de && de.Compensation);
    }

    [Fact]
    public async Task UpdateAsync_WithinTransaction_CommitsUpdateEvent()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Before" };
        await baseRepository.CreateAsync(entity);
        entity.Name = "After";

        var smartRepo = new SmartRepository<TestEntity>(baseRepository, transactionManager);
        transactionManager.Begin();

        // Act
        await smartRepo.UpdateAsync(entity);
        await transactionManager.CommitAsync();
        var updated = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);

        // Assert
        Assert.Equal("After", updated!.Name);
        Assert.Equal(2, events.Count);
        Assert.Contains(events, e => e is UpdateEvent<TestEntity>);
    }

    [Fact]
    public async Task DeleteAsync_WithinTransaction_CommitsDeleteEvent()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ToDeleteTxn" };
        await baseRepository.CreateAsync(entity);

        var smartRepo = new SmartRepository<TestEntity>(baseRepository, transactionManager);
        transactionManager.Begin();

        // Act
        await smartRepo.DeleteAsync(entity);
        await transactionManager.CommitAsync();
        var deleted = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);

        // Assert
        Assert.Null(deleted);
        Assert.Equal(2, events.Count);
        Assert.Contains(events, e => e is DeleteEvent<TestEntity>);
    }

    [Fact]
    public async Task CreateUpdateSucceed_DeleteFails_TriggersCompensationUpdate()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "WillBeUpdated" };
        await baseRepository.CreateAsync(entity); // make sure it exists

        var repo = new FailingRepository(
            entityStore,
            eventStore,
            new GlobalReplayContext(),
            failOnMethod: nameof(Repository<TestEntity>.DeleteAsync)
        );

        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(repo, transactionManager);

        transactionManager.Begin();

        entity.Name = "Updated";
        await smartRepo.UpdateAsync(entity); // commit succeeds
        await smartRepo.DeleteAsync(entity); // commit fails

        // Act
        await Assert.ThrowsAsync<Exception>(() => transactionManager.CommitAsync());
        await transactionManager.RollbackAsync();

        // Assert
        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);

        Assert.Equal(3, events.Count); // Create + Update + Compensation Update
        Assert.Contains(events, e => e is UpdateEvent<TestEntity> ue && ue.Compensation);
    }

    [Fact]
    public async Task CreateAsync_ThenRollback_EmitsNoEvents()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "RollbackCreate" };
        var smartRepo = new SmartRepository<TestEntity>(baseRepository, transactionManager);
        transactionManager.Begin();

        // Act
        await smartRepo.CreateAsync(entity);
        await transactionManager.RollbackAsync();
        var afterRollback = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);

        // Assert
        Assert.Null(afterRollback);
        Assert.Empty(events);
    }

    [Fact]
    public async Task CreateUpdateSucceed_DeleteFails_TriggersMultipleCompensations()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "MultiStep" };
        var repo = new FailingRepository(
            entityStore,
            eventStore,
            new GlobalReplayContext(),
            failOnMethod: nameof(Repository<TestEntity>.DeleteAsync)
        );

        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(repo, transactionManager);

        transactionManager.Begin();

        await smartRepo.CreateAsync(entity); // will commit
        entity.Name = "Updated";
        await smartRepo.UpdateAsync(entity); // will commit
        await smartRepo.DeleteAsync(entity); // will fail

        // Act
        await Assert.ThrowsAsync<Exception>(() => transactionManager.CommitAsync());
        await transactionManager.RollbackAsync();

        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);

        Assert.Equal(4, events.Count); // Create + Update + Compensation Update + Compensation Delete
        Assert.Equal(1, events.Count(e => e is UpdateEvent<TestEntity> ue && ue.Compensation));
        Assert.Equal(1, events.Count(e => e is DeleteEvent<TestEntity> de && de.Compensation));
    }

    [Fact]
    public async Task ReadByIdAsync_ReturnsTrackedUpsertedEntityDuringTransaction()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "UpsertedTemp" };
        var smartRepo = new SmartRepository<TestEntity>(baseRepository, transactionManager);
        transactionManager.Begin();
        await smartRepo.CreateAsync(entity);

        // Act
        var result = await smartRepo.ReadByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("UpsertedTemp", result!.Name);
    }

    [Fact]
    public async Task ReadByIdAsync_ReturnsNullForTrackedDeletedEntityDuringTransaction()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ToDeleteInTxn" };
        await baseRepository.CreateAsync(entity);

        var smartRepo = new SmartRepository<TestEntity>(baseRepository, transactionManager);
        transactionManager.Begin();
        await smartRepo.DeleteAsync(entity);

        // Act
        var result = await smartRepo.ReadByIdAsync(entity.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ReadByFilterAsync_ReturnsTrackedUpsertedEntity()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "FilterHit" };
        var smartRepo = new SmartRepository<TestEntity>(baseRepository, transactionManager);
        transactionManager.Begin();
        await smartRepo.CreateAsync(entity);

        // Act
        var result = await smartRepo.ReadByFilterAsync(x => x.Name == "FilterHit");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("FilterHit", result!.Name);
    }

    [Fact]
    public async Task ReadAllAsync_ReflectsUpsertedAndDeletedEntitiesDuringTransaction()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(baseRepository, transactionManager);
        var created = new TestEntity { Id = Guid.NewGuid(), Name = "Created" };
        var deleted = new TestEntity { Id = Guid.NewGuid(), Name = "Deleted" };
        await baseRepository.CreateAsync(deleted);

        transactionManager.Begin();
        await smartRepo.CreateAsync(created);
        await smartRepo.DeleteAsync(deleted);

        // Act
        var all = await smartRepo.ReadAllAsync();

        // Assert
        Assert.DoesNotContain(all, e => e.Id == deleted.Id);
        Assert.Contains(all, e => e.Id == created.Id);
    }

    [Fact]
    public async Task ReadProjectionByFilterAsync_ReflectsTrackedUpserts()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(baseRepository, transactionManager);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Projectable" };
        transactionManager.Begin();
        await smartRepo.CreateAsync(entity);

        // Act
        var projection = await smartRepo.ReadProjectionByFilterAsync(
            x => x.Name == "Projectable",
            x => x.Name
        );

        // Assert
        Assert.Equal("Projectable", projection);
    }

    [Fact]
    public async Task ReadProjectionByIdAsync_ReturnsNullForTrackedDeletedEntity()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ProjectDelete" };
        await baseRepository.CreateAsync(entity);

        var smartRepo = new SmartRepository<TestEntity>(baseRepository, transactionManager);
        transactionManager.Begin();
        await smartRepo.DeleteAsync(entity);

        // Act
        var projection = await smartRepo.ReadProjectionByIdAsync(entity.Id, e => e.Name);

        // Assert
        Assert.Null(projection);
    }

    private class FailingRepository : Repository<TestEntity>
    {
        private readonly string failOnMethod;

        public FailingRepository(
            IEntityStore entityStore,
            IEventStore eventStore,
            IGlobalReplayContext replayContext,
            string failOnMethod
        )
            : base(entityStore, eventStore, replayContext)
        {
            this.failOnMethod = failOnMethod;
        }

        public override async Task CreateAsync(TestEntity entity, Guid transactionId)
        {
            if (failOnMethod == nameof(CreateAsync))
                throw new Exception("Simulated failure on Create");
            await base.CreateAsync(entity, transactionId);
        }

        public override async Task UpdateAsync(TestEntity entity, Guid transactionId)
        {
            if (failOnMethod == nameof(UpdateAsync))
                throw new Exception("Simulated failure on Update");
            await base.UpdateAsync(entity, transactionId);
        }

        public override async Task DeleteAsync(TestEntity entity, Guid transactionId)
        {
            if (failOnMethod == nameof(DeleteAsync))
                throw new Exception("Simulated failure on Delete");
            await base.DeleteAsync(entity, transactionId);
        }
    }
}
