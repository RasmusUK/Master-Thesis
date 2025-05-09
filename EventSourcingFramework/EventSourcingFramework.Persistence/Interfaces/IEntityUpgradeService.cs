using EventSourcingFramework.Core;

namespace EventSourcingFramework.Persistence.Interfaces;

public interface IEntityUpgradeService
{
    Task MigrateAllEntitiesToLatestVersionAsync<TEntity>()
        where TEntity : IEntity;

    Task MigrateAllEntitiesAsync();
}
