using EventSourcingFramework.Application.Abstractions.Migrations;
using EventSourcingFramework.Infrastructure.Migrations.Services;
using EventSourcingFramework.Infrastructure.MongoDb.Services;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingFramework.Infrastructure.DI;

public static class DomainConfigurationExtensions
{
    public static IServiceCollection AddDomainConfiguration(this IServiceCollection services,
        DomainConfigurator configureDomain)
    {
        var schemaRegistry = new SchemaVersionRegistry();
        var migrationRegistry = new MigrationTypeRegistry();
        var migrator = new EntityMigrator();
        var collectionNameProvider = new EntityCollectionNameProvider(migrationRegistry);

        configureDomain(schemaRegistry, migrationRegistry, migrator, collectionNameProvider);

        services
            .AddSingleton<ISchemaVersionRegistry>(schemaRegistry)
            .AddSingleton<IMigrationTypeRegistry>(migrationRegistry)
            .AddSingleton<IEntityMigrator>(migrator)
            .AddSingleton<IEntityCollectionNameProvider>(_ =>
            {
                MongoDbEventRegistration.RegisterEvents(collectionNameProvider);
                return collectionNameProvider;
            });

        return services;
    }
}