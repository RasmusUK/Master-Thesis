using EventSourcing.Framework.Infrastructure.Shared.Interfaces;
using EventSourcing.Framework.Infrastructure.Shared.Models.Events;
using MongoDB.Bson.Serialization;

namespace EventSourcingFramework.Infrastructure.MongoDb.Services;

public static class MongoDbEventRegistration
{
    public static void RegisterEvents(
        IEntityCollectionNameProvider collectionNameProvider,
        params (Type Type, string CollectionName)[] entities
    )
    {
        foreach (var (entityType, collectionName) in entities)
        {
            RegisterGenericEvent(typeof(MongoCreateEvent<>), entityType);
            RegisterGenericEvent(typeof(MongoUpdateEvent<>), entityType);
            RegisterGenericEvent(typeof(MongoDeleteEvent<>), entityType);

            collectionNameProvider.Register(entityType, collectionName);
        }
    }

    private static void RegisterGenericEvent(Type genericEventType, Type entityType)
    {
        var closedType = genericEventType.MakeGenericType(entityType);

        if (BsonClassMap.IsClassMapRegistered(closedType))
            return;

        var classMap = new BsonClassMap(closedType);
        classMap.AutoMap();
        classMap.SetDiscriminator(closedType.FullName);
        classMap.SetDiscriminatorIsRequired(true);

        BsonClassMap.RegisterClassMap(classMap);
    }
}
