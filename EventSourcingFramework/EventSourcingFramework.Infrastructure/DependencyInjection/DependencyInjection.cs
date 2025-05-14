using EventSourcing.Framework.Infrastructure.Shared.Configuration.Adapters;
using EventSourcing.Framework.Infrastructure.Shared.Configuration.Options;
using EventSourcingFramework.Application;
using EventSourcingFramework.Application.Abstractions;
using EventSourcingFramework.Application.Abstractions.Snapshots;
using EventSourcingFramework.Application.UseCases.EntityHistory;
using EventSourcingFramework.Application.UseCases.PersonalData;
using EventSourcingFramework.Application.UseCases.Replay;
using EventSourcingFramework.Application.UseCases.ReplayContext;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Abstractions;
using EventSourcingFramework.Infrastructure.Abstractions.EventStore;
using EventSourcingFramework.Infrastructure.Abstractions.Migrations;
using EventSourcingFramework.Infrastructure.Abstractions.MongoDb;
using EventSourcingFramework.Infrastructure.Migrations;
using EventSourcingFramework.Infrastructure.MongoDb;
using EventSourcingFramework.Infrastructure.Repositories;
using EventSourcingFramework.Infrastructure.Repositories.Interfaces;
using EventSourcingFramework.Infrastructure.Snapshots;
using EventSourcingFramework.Infrastructure.Stores.EntityStore;
using EventSourcingFramework.Infrastructure.Stores.EventStore;
using EventSourcingFramework.Infrastructure.Stores.PersonalDataStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace EventSourcingFramework.Infrastructure.DependencyInjection;

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
            .AddSingleton<IEventSourcingSettings>(sp =>
                sp.GetRequiredService<IOptions<EventSourcingOptions>>().Value)
            .AddSingleton<ISnapshotSettings, SnapshotSettingsAdapter>()
            .AddSingleton<IReplayEnvironmentSwitcher, ReplayEnvironmentSwitcher>()
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
