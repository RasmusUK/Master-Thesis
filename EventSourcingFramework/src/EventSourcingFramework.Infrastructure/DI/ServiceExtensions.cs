using EventSourcingFramework.Application.DI;
using EventSourcingFramework.Infrastructure.Http.DI;
using EventSourcingFramework.Infrastructure.Migrations.DI;
using EventSourcingFramework.Infrastructure.MongoDb.DI;
using EventSourcingFramework.Infrastructure.Repositories.DI;
using EventSourcingFramework.Infrastructure.Shared.DI;
using EventSourcingFramework.Infrastructure.Snapshots.DI;
using EventSourcingFramework.Infrastructure.Stores.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingFramework.Infrastructure.DI;

public static class ServiceExtensions
{
    public static IServiceCollection AddEventSourcing(
        this IServiceCollection services,
        IConfiguration configuration,
        DomainConfigurator domainConfigurator
    )
    {
        services
            .AddFrameworkOptions(configuration)
            .AddStores()
            .AddSnapshots()
            .AddRepositories()
            .AddMongoDb()
            .AddMigrations()
            .AddHttp()
            .AddApplications()
            .AddDomainConfiguration(domainConfigurator);

        return services;
    }
}
