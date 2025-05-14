using EventSourcingFramework.Infrastructure.Repositories.Exceptions;
using EventSourcingFramework.Infrastructure.Repositories.Interfaces;
using EventSourcingFramework.Infrastructure.Repositories.Services;
using EventSourcingFramework.Test.Utilities;

namespace EventSourcingFramework.Infrastructure.Test.Integration.Repositories;

public class TransactionManagerTests
{
    private readonly ITransactionManager transactionManager = new TransactionManager();

    [Fact]
    public void Begin_WhenAlreadyActive_Throws()
    {
        transactionManager.Begin();
        Assert.Throws<TransactionException>(() => transactionManager.Begin());
    }

    [Fact]
    public void Enlist_WithoutBegin_Throws()
    {
        Assert.Throws<TransactionException>(
            () => transactionManager.Enlist(() => Task.CompletedTask, () => Task.CompletedTask)
        );
    }

    [Fact]
    public async Task CommitAsync_WithoutBegin_Throws()
    {
        await Assert.ThrowsAsync<TransactionException>(() => transactionManager.CommitAsync());
    }

    [Fact]
    public async Task RollbackAsync_WithoutBegin_Throws()
    {
        await Assert.ThrowsAsync<TransactionException>(() => transactionManager.RollbackAsync());
    }

    [Fact]
    public async Task Commit_ExecutesAllCommitActions()
    {
        // Arrange
        transactionManager.Begin();
        var log = new List<string>();

        transactionManager.Enlist(
            () =>
            {
                log.Add("commit1");
                return Task.CompletedTask;
            },
            () =>
            {
                log.Add("rollback1");
                return Task.CompletedTask;
            }
        );
        transactionManager.Enlist(
            () =>
            {
                log.Add("commit2");
                return Task.CompletedTask;
            },
            () =>
            {
                log.Add("rollback2");
                return Task.CompletedTask;
            }
        );

        // Act
        await transactionManager.CommitAsync();

        // Assert
        Assert.Equal(new[] { "commit1", "commit2" }, log);
    }

    [Fact]
    public async Task Rollback_ExecutesOnlyCommittedRollbacks_InReverseOrder()
    {
        // Arrange
        transactionManager.Begin();
        var log = new List<string>();

        transactionManager.Enlist(
            () =>
            {
                log.Add("commit1");
                return Task.CompletedTask;
            },
            () =>
            {
                log.Add("rollback1");
                return Task.CompletedTask;
            }
        );
        transactionManager.Enlist(
            () => throw new Exception("commit2 failed"),
            () =>
            {
                log.Add("rollback2");
                return Task.CompletedTask;
            }
        );

        // Act
        try
        {
            await transactionManager.CommitAsync();
        }
        catch
        {
            await transactionManager.RollbackAsync();
        }

        // Assert
        Assert.Equal(new[] { "commit1", "rollback1" }, log); // only rollback for commit1 should run
    }

    [Fact]
    public async Task TrackUpsertedEntity_CanBeRetrieved()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "TrackedUpsert" };
        transactionManager.Begin();

        // Act
        transactionManager.TrackUpsertedEntity(entity);
        var tracked = transactionManager.GetTrackedUpsertedEntities<TestEntity>().ToList();

        // Assert
        Assert.Single(tracked);
        Assert.Equal(entity.Id, tracked[0].Id);
    }

    [Fact]
    public async Task TrackDeletedEntity_CanBeRetrieved()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ToDelete" };
        transactionManager.Begin();

        // Act
        transactionManager.TrackDeletedEntity(entity);
        var tracked = transactionManager.GetTrackedDeletedEntities<TestEntity>().ToList();

        // Assert
        Assert.Single(tracked);
        Assert.Equal(entity.Id, tracked[0].Id);
    }

    [Fact]
    public void GetTrackedEntities_WithoutBegin_ReturnsEmpty()
    {
        var upserts = transactionManager.GetTrackedUpsertedEntities<TestEntity>();
        var deletes = transactionManager.GetTrackedDeletedEntities<TestEntity>();

        Assert.Empty(upserts);
        Assert.Empty(deletes);
    }

    [Fact]
    public async Task Commit_ClearsState()
    {
        transactionManager.Begin();
        transactionManager.TrackUpsertedEntity(new TestEntity { Id = Guid.NewGuid(), Name = "X" });
        transactionManager.Enlist(() => Task.CompletedTask, () => Task.CompletedTask);

        await transactionManager.CommitAsync();

        Assert.Empty(transactionManager.GetTrackedUpsertedEntities<TestEntity>());
        Assert.False(transactionManager.IsActive);
    }

    [Fact]
    public async Task Rollback_ClearsState()
    {
        transactionManager.Begin();
        transactionManager.TrackDeletedEntity(new TestEntity { Id = Guid.NewGuid(), Name = "Y" });
        transactionManager.Enlist(() => Task.CompletedTask, () => Task.CompletedTask);

        await transactionManager.RollbackAsync();

        Assert.Empty(transactionManager.GetTrackedDeletedEntities<TestEntity>());
        Assert.False(transactionManager.IsActive);
    }
}
