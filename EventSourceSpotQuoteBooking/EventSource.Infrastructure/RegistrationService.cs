using EventSource.Persistence.Events;
using MongoDB.Bson.Serialization;

namespace EventSource.Infrastructure;

public class RegistrationService
{
    private static bool registered;

    public static void RegisterEntities(params Type[] entityTypes)
    {
        if (registered)
            return;
        registered = true;

        foreach (var entityType in entityTypes)
        {
            RegisterGenericEvent(typeof(CreateEvent<>), entityType);
            RegisterGenericEvent(typeof(UpdateEvent<>), entityType);
            RegisterGenericEvent(typeof(DeleteEvent<>), entityType);
        }
    }

    private static void RegisterGenericEvent(Type genericEventType, Type entityType)
    {
        var closedType = genericEventType.MakeGenericType(entityType);

        if (!BsonClassMap.IsClassMapRegistered(closedType))
        {
            var classMap = new BsonClassMap(closedType);
            classMap.AutoMap();
            classMap.SetDiscriminator(closedType.FullName);
            classMap.SetDiscriminatorIsRequired(true);

            BsonClassMap.RegisterClassMap(classMap);
        }
    }
}
