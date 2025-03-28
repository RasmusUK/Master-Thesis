using EventSource.Application;
using EventSource.Application.Interfaces;
using EventSource.Core.Interfaces;
using EventSource.Persistence;
using EventSource.Persistence.Interfaces;
using EventSource.Persistence.Stores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventSource.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEventSourcing(
        this IServiceCollection services,
        IConfiguration configuration
    ) => services.AddApplicationServices().AddPersistence(configuration);

    private static IServiceCollection AddApplicationServices(this IServiceCollection services) =>
        services
            .AddSingleton<IEntityHistoryService, EntityHistoryService>()
            .AddSingleton<IPersonalDataInterceptor, PersonalDataInterceptor>()
            .AddSingleton<IReplayService, ReplayService>()
            .AddSingleton<IEventProcessor, EventProcessor>();

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    ) =>
        services
            .Configure<MongoDbOptions>(configuration.GetSection(MongoDbOptions.MongoDb))
            .AddSingleton<IMongoDbService, MongoDbService>()
            .AddSingleton<IEntityStore, MongoDbEntityStore>()
            .AddSingleton<IMongoDbEntityStore, MongoDbEntityStore>()
            .AddSingleton<IEventStore, MongoDbEventStore>()
            .AddSingleton<IMongoDbEventStore, MongoDbEventStore>()
            .AddSingleton<IPersonalDataStore, MongoDbPersonalDataStore>()
            .AddSingleton(typeof(IRepository<>), typeof(Repository<>));
}
