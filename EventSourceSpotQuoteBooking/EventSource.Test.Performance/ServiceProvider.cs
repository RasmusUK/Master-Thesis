using EventSource.Infrastructure;
using EventSource.Persistence;
using EventSource.Persistence.Interfaces;
using EventSource.Test.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventSource.Test.Performance;

public static class ServiceProvider
{
    public static IServiceProvider BuildServiceProviderWithSettings(
        Dictionary<string, string>? overrides = null
    )
    {
        var defaultSettings = new Dictionary<string, string>
        {
            ["EventSourcing:EnableEventStore"] = "true",
            ["EventSourcing:EnableEntityStore"] = "true",
            ["EventSourcing:EnablePersonalDataStore"] = "true",
            ["EventSourcing:Snapshot:Enabled"] = "false",
            ["EventSourcing:Snapshot:Trigger:Mode"] = "Either",
            ["EventSourcing:Snapshot:Trigger:Frequency"] = "Week",
            ["EventSourcing:Snapshot:Trigger:EventThreshold"] = "1000",
            ["EventSourcing:Snapshot:Retention:Strategy"] = "Count",
            ["EventSourcing:Snapshot:Retention:MaxCount"] = "20",
            ["EventSourcing:Snapshot:Retention:MaxAgeDays"] = "180",

            ["MongoDb:EventStore:ConnectionString"] = "mongodb://localhost:27018",
            ["MongoDb:EventStore:DatabaseName"] = "EventStore",

            ["MongoDb:EntityStore:ConnectionString"] = "mongodb://localhost:27018",
            ["MongoDb:EntityStore:DatabaseName"] = "EntityStore",

            ["MongoDb:DebugEntityStore:ConnectionString"] = "mongodb://localhost:27018",
            ["MongoDb:DebugEntityStore:DatabaseName"] = "EntityStore_debug",

            ["MongoDb:PersonalDataStore:ConnectionString"] = "mongodb://localhost:27018",
            ["MongoDb:PersonalDataStore:DatabaseName"] = "PersonalDataStore",
        };

        if (overrides != null)
        {
            foreach (var (key, value) in overrides)
                defaultSettings[key] = value;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(defaultSettings)
            .Build();

        var services = new ServiceCollection();
        services.AddEventSourcing(configuration);

        services.AddSingleton<ISchemaVersionRegistry>(_ =>
        {
            var registry = new SchemaVersionRegistry();
            return registry;
        });

        services.AddSingleton<IMigrationTypeRegistry>(_ =>
        {
            var registry = new MigrationTypeRegistry();
            return registry;
        });

        services.AddSingleton<IEntityMigrator>(_ =>
        {
            var migrator = new EntityMigrator();
            return migrator;
        });

        services.AddSingleton<IEntityCollectionNameProvider>(sp =>
        {
            var registry = sp.GetRequiredService<IMigrationTypeRegistry>();
            var collectionNameProvider = new EntityCollectionNameProvider(registry);

            RegistrationService.RegisterEntities(
                collectionNameProvider,
                (typeof(TestEntity), "TestEntity")
            );

            return collectionNameProvider;
        });

        return services.BuildServiceProvider();
    }
}
