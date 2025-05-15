using EventSourcingFramework.Infrastructure.DI;
using EventSourcingFramework.Infrastructure.MongoDb.Services;
using EventSourcingFramework.Test.Utilities.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingFramework.Test.Utilities;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        services.AddEventSourcing(configuration, (schema, migrations, migrator, mongoDbRegistrationService) =>
        {
            schema.Register(typeof(TestEntity), 3);

            migrations.Register<TestEntity>(1, typeof(TestEntity1));
            migrations.Register<TestEntity>(2, typeof(TestEntity2));
            migrations.Register<TestEntity>(3, typeof(TestEntity));

            migrator.Register<TestEntity1, TestEntity2>(1, v1 => new TestEntity2
            {
                Id = v1.Id,
                FirstName = v1.FirstName,
                SurName = v1.LastName
            });

            migrator.Register<TestEntity2, TestEntity>(2, v2 => new TestEntity
            {
                Id = v2.Id,
                Name = $"{v2.FirstName} - {v2.SurName}"
            });

            mongoDbRegistrationService.Register(
                (typeof(TestEntity), "TestEntity"),
                (typeof(PersonEntity), "PersonEntity")
            );
        });
    }
}