using EventSourcingFramework.Infrastructure;
using EventSourcingFramework.Persistence;
using EventSourcingFramework.Persistence.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingFramework.Test.Utilities;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddEventSourcing(configuration);

        services.AddSingleton<ISchemaVersionRegistry>(_ =>
        {
            var registry = new SchemaVersionRegistry();
            registry.Register(typeof(TestEntity), 3);
            return registry;
        });

        services.AddSingleton<IMigrationTypeRegistry>(_ =>
        {
            var registry = new MigrationTypeRegistry();
            registry.Register<TestEntity>(1, typeof(TestEntity1));
            registry.Register<TestEntity>(2, typeof(TestEntity2));
            registry.Register<TestEntity>(3, typeof(TestEntity));
            return registry;
        });

        services.AddSingleton<IEntityMigrator>(_ =>
        {
            var migrator = new EntityMigrator();
            migrator.Register<TestEntity1, TestEntity2>(
                1,
                v1 => new TestEntity2
                {
                    Id = v1.Id,
                    FirstName = v1.FirstName,
                    SurName = v1.LastName,
                }
            );
            migrator.Register<TestEntity2, TestEntity>(
                2,
                v2 => new TestEntity { Id = v2.Id, Name = $"{v2.FirstName} - {v2.SurName}" }
            );
            return migrator;
        });

        services.AddSingleton<IEntityCollectionNameProvider>(sp =>
        {
            var registry = sp.GetRequiredService<IMigrationTypeRegistry>();
            var collectionNameProvider = new EntityCollectionNameProvider(registry);

            RegistrationService.RegisterEntities(
                collectionNameProvider,
                (typeof(TestEntity), "TestEntity"),
                (typeof(PersonEntity), "PersonEntity")
            );

            return collectionNameProvider;
        });
    }
}
