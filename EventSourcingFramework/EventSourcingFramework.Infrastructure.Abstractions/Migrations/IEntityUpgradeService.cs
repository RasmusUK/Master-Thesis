using EventSourcingFramework.Core;

namespace EventSourcingFramework.Infrastructure.Abstractions.Migrations;

public interface IEntityUpgradeService
{
    Task MigrateAllEntitiesToLatestVersionAsync<TEntity>()
        where TEntity : IEntity;

    Task MigrateAllEntitiesAsync();
}
