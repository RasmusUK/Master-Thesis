using EventSourcingFramework.Application.Abstractions.EventStore;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Stores.EventStore;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingFramework.Infrastructure.Stores.DI;

public static class StoresExtensions
{
    public static IServiceCollection AddStores(this IServiceCollection services)
    {
        services
            .AddSingleton<IEventSequenceGenerator, EventSequenceGenerator>()
            .AddSingleton<IEntityStore, EntityStore.EntityStore>()
            .AddSingleton<IEventStore, EventStore.EventStore>()
            .AddSingleton<IPersonalDataStore, PersonalDataStore.PersonalDataStore>()
            .AddSingleton<IApiResponseStore, ApiResponseStore.ApiResponseStore>();

        return services;
    }
}