using EventSourcingFramework.Core;
using EventSourcingFramework.Core.Models;
using EventSourcingFramework.Core.Models.Entity;

namespace EventSourcingFramework.Application.Abstractions.Migrations;

public interface IEntityUpgradeService
{
    Task MigrateAllEntitiesToLatestVersionAsync<TEntity>()
        where TEntity : IEntity;

    Task MigrateAllEntitiesAsync();
}
