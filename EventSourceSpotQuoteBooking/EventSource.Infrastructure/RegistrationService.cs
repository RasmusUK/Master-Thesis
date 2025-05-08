using EventSource.Persistence.Events;
using EventSource.Persistence.Interfaces;
using MongoDB.Bson.Serialization;

namespace EventSource.Infrastructure;

public static class RegistrationService
{
    public static void RegisterEntities(
        IEntityCollectionNameProvider collectionNameProvider,
        params (Type Type, string CollectionName)[] entities
    )
    {
        foreach (var (entityType, collectionName) in entities)
        {
            RegisterGenericEvent(typeof(CreateEvent<>), entityType);
            RegisterGenericEvent(typeof(UpdateEvent<>), entityType);
            RegisterGenericEvent(typeof(DeleteEvent<>), entityType);

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
