using EventSourcingFramework.Infrastructure.DI;
using EventSourcingFramework.Infrastructure.MongoDb.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingFramework.Test.Performance;

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
            ["MongoDb:PersonalDataStore:DatabaseName"] = "PersonalDataStore"
        };

        if (overrides != null)
            foreach (var (key, value) in overrides)
                defaultSettings[key] = value;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(defaultSettings)
            .Build();

        var services = new ServiceCollection();
        services.AddEventSourcing(configuration, (schema, migrations, migrator, mongoDbRegistrationService) =>
        {
            schema.Register(typeof(TestEntity), 1);

            migrations.Register<TestEntity>(1, typeof(TestEntity));

            mongoDbRegistrationService.Register(
                (typeof(TestEntity), "TestEntity")
            );
        });

        return services.BuildServiceProvider();
    }
}