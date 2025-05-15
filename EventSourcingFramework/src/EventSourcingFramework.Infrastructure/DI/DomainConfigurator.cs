using EventSourcingFramework.Application.Abstractions.Migrations;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;

namespace EventSourcingFramework.Infrastructure.DI;

public delegate void DomainConfigurator(
    ISchemaVersionRegistry schemaRegistry,
    IMigrationTypeRegistry migrationRegistry,
    IEntityMigrator migrator,
    IMongoDbRegistrationService mongoDbRegistrationService
);