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
        var mongoDbRegistrationService = new MongoDbRegistrationService();
        
        configureDomain(schemaRegistry, migrationRegistry, migrator, mongoDbRegistrationService);

        var collectionNameProvider = new EntityCollectionNameProvider(migrationRegistry);
        var registered = mongoDbRegistrationService.GetRegistered();
        foreach (var (type, collectionName) in registered)
            collectionNameProvider.Register(type, collectionName);
        
        services
            .AddSingleton<ISchemaVersionRegistry>(schemaRegistry)
            .AddSingleton<IMigrationTypeRegistry>(migrationRegistry)
            .AddSingleton<IEntityMigrator>(migrator)
            .AddSingleton<IEntityCollectionNameProvider>(collectionNameProvider)
            .AddSingleton<IMongoDbRegistrationService>(mongoDbRegistrationService);

        return services;
    }
}