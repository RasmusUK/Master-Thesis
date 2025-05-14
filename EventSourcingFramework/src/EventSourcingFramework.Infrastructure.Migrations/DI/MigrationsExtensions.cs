using EventSourcingFramework.Application.Abstractions.Migrations;
using EventSourcingFramework.Infrastructure.Migrations.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingFramework.Infrastructure.Migrations.DI;

public static class MigrationsExtensions
{
    public static IServiceCollection AddMigrations(this IServiceCollection services)
    {
        services
            .AddSingleton<IMigrationTypeRegistry, MigrationTypeRegistry>()
            .AddSingleton<IEntityMigrator, EntityMigrator>()
            .AddSingleton<IEntityUpgradeService, EntityUpgradeService>();

        return services;
    }
}