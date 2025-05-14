using EventSourcing.Framework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Application.Abstractions.Migrations;

namespace EventSourcingFramework.Infrastructure.DI;

public delegate void DomainConfigurator(
    ISchemaVersionRegistry schemaRegistry,
    IMigrationTypeRegistry migrationRegistry,
    IEntityMigrator migrator,
    IEntityCollectionNameProvider collectionNameProvider
);
