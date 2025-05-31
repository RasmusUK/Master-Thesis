using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Models.Events;
using MongoDB.Bson.Serialization;

namespace EventSourcingFramework.Infrastructure.MongoDb.Services;

public class MongoDbRegistrationService : IMongoDbRegistrationService
{
    private readonly List <(Type Type, string CollectionName)> registered = new();
    
    public void Register(
        params (Type Type, string CollectionName)[] entities
    )
    {
        foreach (var (entityType, collectionName) in entities)
        {
            registered.Add((entityType, collectionName));
            RegisterGenericEvent(typeof(CreateEvent<>), entityType);
            RegisterGenericEvent(typeof(UpdateEvent<>), entityType);
            RegisterGenericEvent(typeof(DeleteEvent<>), entityType);
        }
    }

    public List<(Type Type, string CollectionName)> GetRegistered() => registered;

    private void RegisterGenericEvent(Type genericEventType, Type entityType)
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

