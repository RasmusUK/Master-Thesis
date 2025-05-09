using EventSourcingFramework.Application;
using EventSourcingFramework.Application.Interfaces;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Core.Options;
using EventSourcingFramework.Infrastructure.Interfaces;
using EventSourcingFramework.Persistence;
using EventSourcingFramework.Persistence.Interfaces;
using EventSourcingFramework.Persistence.Options;
using EventSourcingFramework.Persistence.Snapshot;
using EventSourcingFramework.Persistence.Stores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace EventSourcingFramework.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEventSourcing(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        try
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }
        catch (BsonSerializationException e)
        {
            if (!e.Message.Contains("There is already a serializer registered for type Guid"))
                throw;
        }

        return services
            .Configure<MongoDbOptions>(configuration.GetSection(MongoDbOptions.MongoDb))
            .Configure<EventSourcingOptions>(
                configuration.GetSection(EventSourcingOptions.EventSourcing)
            )
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
