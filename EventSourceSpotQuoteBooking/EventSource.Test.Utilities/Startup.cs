using EventSource.Infrastructure;
using EventSource.Persistence;
using EventSource.Persistence.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventSource.Test.Utilities;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddEventSourcing(configuration);

        var collectionNameProvider = new EntityCollectionNameProvider();

        RegistrationService.RegisterEntities(
            collectionNameProvider,
            (typeof(TestEntity), "TestEntity")
        );

        services.AddSingleton<IEntityCollectionNameProvider>(collectionNameProvider);
    }
}
