using EventSourcingFramework.Application.Abstractions.Replay;
using EventSourcingFramework.Infrastructure.MongoDb.Services;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace EventSourcingFramework.Infrastructure.MongoDb.DI;

public static class MongoDbExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services)
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

        services
            .AddSingleton<IMongoDbService, MongoDbService>()
            .AddSingleton<IReplayEnvironmentSwitcher, ReplayEnvironmentSwitcher>();

        return services;
    }
}