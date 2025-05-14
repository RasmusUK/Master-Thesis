using EventSourcingFramework.Application.Abstractions.Snapshots;
using EventSourcingFramework.Infrastructure.Snapshots.Config;
using EventSourcingFramework.Infrastructure.Snapshots.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingFramework.Infrastructure.Snapshots.DI;

public static class SnapshotsExtensions
{
    public static IServiceCollection AddSnapshots(this IServiceCollection services)
    {
        services
            .AddSingleton<ISnapshotSettings, SnapshotSettingsAdapter>()
            .AddSingleton<ISnapshotService, SnapshotService>();
        
        return services;
    }
}