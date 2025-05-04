using EventSource.Core;

namespace EventSource.Persistence.Interfaces;

public interface IEntityUpgradeService
{
    Task MigrateAllEntitiesToLatestVersionAsync<TEntity>()
        where TEntity : IEntity;

    Task MigrateAllEntitiesAsync();
}
