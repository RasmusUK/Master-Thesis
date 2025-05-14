using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Application.UseCases.ReplayContext;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Repositories.Services;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Models.Events;
using EventSourcingFramework.Test.Utilities;
using EventSourcingFramework.Test.Utilities.Models;

namespace EventSourcingFramework.Infrastructure.Test.Integration.Repositories;

[Collection("Integration")]
public class SmartRepositoryTests : MongoIntegrationTestBase
{
    private readonly Repository<TestEntity> repository;
    private readonly IEventStore eventStore;
    private readonly IEntityStore entityStore;

    public SmartRepositoryTests(
        IMongoDbService mongoDbService,
        IGlobalReplayContext replayContext,
        Repository<TestEntity> repository,
        IEventStore eventStore,
        IEntityStore entityStore
    )
        : base(mongoDbService, replayContext)
    {
        this.repository = repository;
        this.eventStore = eventStore;
        this.entityStore = entityStore;
    }

    [Fact]
    public async Task CreateAsync_WithinTransaction_CommitsEventAndEntity()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);
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
        Assert.IsType<MongoCreateEvent<TestEntity>>(events.First());
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

        await smartRepo.CreateAsync(entity); 
        await smartRepo.UpdateAsync(entity);

        // Act
        await Assert.ThrowsAsync<Exception>(() => transactionManager.CommitAsync());
        await transactionManager.RollbackAsync();

        // Assert
        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);

        Assert.Equal(2, events.Count);
        Assert.Contains(events, e => e is MongoCreateEvent<TestEntity>);
        Assert.Contains(events, e => e is MongoDeleteEvent<TestEntity> de && de.Compensation);
    }
    
    [Fact]
    public async Task CreateSucceeds_UpdateFails_TriggersCompensationCreate()
    {
        // Arrange
        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "TestEntity" };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "TestEntity" };

        var repo = new FailingRepository(
            entityStore,
            eventStore,
            new GlobalReplayContext(),
            failOnMethod: nameof(Repository<TestEntity>.UpdateAsync)
        );

        await repo.CreateAsync(entity1);
        
        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(repo, transactionManager);
        
        transactionManager.Begin();
        
        await smartRepo.DeleteAsync(entity1);
        await smartRepo.CreateAsync(entity2);
        await smartRepo.UpdateAsync(entity2);

        // Act
        await Assert.ThrowsAsync<Exception>(() => transactionManager.CommitAsync());
        await transactionManager.RollbackAsync();

        // Assert
        var events = await eventStore.GetEventsByEntityIdAsync(entity1.Id);

        Assert.Equal(3, events.Count);
        Assert.Contains(events, e => e is MongoCreateEvent<TestEntity>);
        Assert.Contains(events, e => e is MongoDeleteEvent<TestEntity>);
        Assert.Contains(events, e => e is MongoCreateEvent<TestEntity> de && de.Compensation);
    }

    [Fact]
    public async Task UpdateAsync_WithinTransaction_CommitsMongoUpdateEvent()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Before" };
        await repository.CreateAsync(entity);
        entity.Name = "After";

        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);
        transactionManager.Begin();

        // Act
        await smartRepo.UpdateAsync(entity);
        await transactionManager.CommitAsync();
        var updated = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);

        // Assert
        Assert.Equal("After", updated!.Name);
        Assert.Equal(2, events.Count);
        Assert.Contains(events, e => e is MongoUpdateEvent<TestEntity>);
    }

    [Fact]
    public async Task DeleteAsync_WithinTransaction_CommitsMongoDeleteEvent()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ToDeleteTxn" };
        await repository.CreateAsync(entity);

        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);
        transactionManager.Begin();

        // Act
        await smartRepo.DeleteAsync(entity);
        await transactionManager.CommitAsync();
        var deleted = await entityStore.GetEntityByIdAsync<TestEntity>(entity.Id);
        var events = await eventStore.GetEventsByEntityIdAsync(entity.Id);

        // Assert
        Assert.Null(deleted);
        Assert.Equal(2, events.Count);
        Assert.Contains(events, e => e is MongoDeleteEvent<TestEntity>);
    }

    [Fact]
    public async Task CreateUpdateSucceed_DeleteFails_TriggersCompensationUpdate()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "WillBeUpdated" };
        await repository.CreateAsync(entity); // make sure it exists

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
        Assert.Contains(events, e => e is MongoUpdateEvent<TestEntity> ue && ue.Compensation);
    }

    [Fact]
    public async Task CreateAsync_ThenRollback_EmitsNoEvents()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "RollbackCreate" };
        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);
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
        Assert.Equal(1, events.Count(e => e is MongoUpdateEvent<TestEntity> ue && ue.Compensation));
        Assert.Equal(1, events.Count(e => e is MongoDeleteEvent<TestEntity> de && de.Compensation));
    }

    [Fact]
    public async Task ReadByIdAsync_ReturnsTrackedUpsertedEntityDuringTransaction()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "UpsertedTemp" };
        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);
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
        await repository.CreateAsync(entity);

        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);
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
        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);
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
        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);
        var created = new TestEntity { Id = Guid.NewGuid(), Name = "Created" };
        var deleted = new TestEntity { Id = Guid.NewGuid(), Name = "Deleted" };
        await repository.CreateAsync(deleted);

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
        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);
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
        await repository.CreateAsync(entity);

        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);
        transactionManager.Begin();
        await smartRepo.DeleteAsync(entity);

        // Act
        var projection = await smartRepo.ReadProjectionByIdAsync(entity.Id, e => e.Name);

        // Assert
        Assert.Null(projection);
    }
    
    [Fact]
    public async Task ReadAllByFilterAsync_RespectsTransactionState()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);

        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "Match" };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "FilteredOut" };

        await repository.CreateAsync(entity1);
        await repository.CreateAsync(entity2);

        transactionManager.Begin();
        await smartRepo.DeleteAsync(entity1);

        // Act
        var results = await smartRepo.ReadAllByFilterAsync(x => x.Name.Contains("Match"));

        // Assert
        Assert.DoesNotContain(results, e => e.Id == entity1.Id);
    }

    [Fact]
    public async Task ReadAllProjectionsAsync_ReturnsExpectedProjections()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ProjAll" };
        transactionManager.Begin();
        await smartRepo.CreateAsync(entity);

        // Act
        var projections = await smartRepo.ReadAllProjectionsAsync(e => e.Name);

        // Assert
        Assert.Contains("ProjAll", projections);
    }
    
    [Fact]
    public async Task ReadAllProjectionsByFilterAsync_ReturnsExpectedFilteredProjections()
    {
        // Arrange
        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ProjFilter" };
        transactionManager.Begin();
        await smartRepo.CreateAsync(entity);

        // Act
        var results = await smartRepo.ReadAllProjectionsByFilterAsync(
            x => x.Name,
            x => x.Name == "ProjFilter"
        );

        // Assert
        Assert.Single(results);
        Assert.Equal("ProjFilter", results.First());
    }
    
    [Fact]
    public async Task ReadProjectionByIdAsync_ReturnsProjectionForUpsertedEntity()
    {
        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Projected" };

        transactionManager.Begin();
        await smartRepo.CreateAsync(entity);

        var result = await smartRepo.ReadProjectionByIdAsync(entity.Id, e => e.Name);

        Assert.Equal("Projected", result);
    }
    
    [Fact]
    public async Task ReadProjectionByFilterAsync_ReturnsNullForDeletedEntity()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ToDelete" };
        await repository.CreateAsync(entity);

        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);

        transactionManager.Begin();
        await smartRepo.DeleteAsync(entity);

        var result = await smartRepo.ReadProjectionByFilterAsync(
            x => x.Id == entity.Id,
            x => x.Name
        );

        Assert.Null(result);
    }
    
    [Fact]
    public async Task ReadAllByFilterAsync_ExcludesDeletedEntities()
    {
        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);

        var toKeep = new TestEntity { Id = Guid.NewGuid(), Name = "Keep" };
        var toDelete = new TestEntity { Id = Guid.NewGuid(), Name = "Delete" };

        await repository.CreateAsync(toKeep);
        await repository.CreateAsync(toDelete);

        transactionManager.Begin();
        await smartRepo.DeleteAsync(toDelete);

        var results = await smartRepo.ReadAllByFilterAsync(x => true);

        Assert.DoesNotContain(results, e => e.Id == toDelete.Id);
        Assert.Contains(results, e => e.Id == toKeep.Id);
    }

    [Fact]
    public async Task ReadAllProjectionsAsync_ExcludesProjectionsOfDeletedEntities()
    {
        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);

        var toDelete = new TestEntity { Id = Guid.NewGuid(), Name = "ProjToDelete" };
        await repository.CreateAsync(toDelete);

        transactionManager.Begin();
        await smartRepo.DeleteAsync(toDelete);

        var projections = await smartRepo.ReadAllProjectionsAsync(e => e.Name);

        Assert.DoesNotContain("ProjToDelete", projections);
    }

    [Fact]
    public async Task ReadAllProjectionsByFilterAsync_ExcludesDeletedAndAppliesFilter()
    {
        var transactionManager = new TransactionManager();
        var smartRepo = new SmartRepository<TestEntity>(repository, transactionManager);

        var matching = new TestEntity { Id = Guid.NewGuid(), Name = "FilterKeep" };
        var toDelete = new TestEntity { Id = Guid.NewGuid(), Name = "FilterDelete" };

        await repository.CreateAsync(matching);
        await repository.CreateAsync(toDelete);

        transactionManager.Begin();
        await smartRepo.DeleteAsync(toDelete);

        var results = await smartRepo.ReadAllProjectionsByFilterAsync(
            x => x.Name,
            x => x.Name.StartsWith("Filter")
        );

        Assert.Contains("FilterKeep", results);
        Assert.DoesNotContain("FilterDelete", results);
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
