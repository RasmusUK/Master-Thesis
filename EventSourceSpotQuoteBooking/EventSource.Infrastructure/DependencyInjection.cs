using EventSource.Application;
using EventSource.Application.Interfaces;
using EventSource.Core.Interfaces;
using EventSource.Infrastructure.Interfaces;
using EventSource.Persistence;
using EventSource.Persistence.Interfaces;
using EventSource.Persistence.Stores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace EventSource.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEventSourcing(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        return services
            .Configure<MongoDbOptions>(configuration.GetSection(MongoDbOptions.MongoDb))
            .AddSingleton<IPersonalDataStore, PersonalDataStore>()
            .AddSingleton<IPersonalDataService, PersonalDataService>()
            .AddSingleton<IEntityHistoryService, EntityHistoryService>()
            .AddSingleton<IReplayService, ReplayService>()
            .AddSingleton<IGlobalReplayContext, GlobalReplayContext>()
            .AddSingleton<IMigrationTypeRegistry, MigrationTypeRegistry>()
            .AddSingleton<IEntityMigrator, EntityMigrator>()
            .AddSingleton<IEntityUpgradeService, EntityUpgradeService>()
            .AddSingleton<IEventSequenceGenerator, EventSequenceGenerator>()
            .AddSingleton<IMongoDbService, MongoDbService>()
            .AddSingleton<IEntityStore, EntityStore>()
            .AddSingleton<IEventStore, EventStore>()
            .AddSingleton(typeof(Repository<>))
            .AddSingleton<ISnapshotService, SnapshotService>()
            .AddScoped<ITransactionManager, TransactionManager>()
            .AddScoped(typeof(IRepository<>), typeof(SmartRepository<>));
    }
}
