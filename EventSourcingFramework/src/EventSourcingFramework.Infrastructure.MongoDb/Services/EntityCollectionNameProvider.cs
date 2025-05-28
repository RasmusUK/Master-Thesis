using EventSourcingFramework.Infrastructure.Shared.Interfaces;

namespace EventSourcingFramework.Infrastructure.MongoDb.Services;

public class EntityCollectionNameProvider : IEntityCollectionNameProvider
{
    private readonly Dictionary<Type, string> collectionNames = new();
    private readonly Dictionary<Type, Type> migrationToBaseTypes = new();
    
    public void Register(Type type, string collectionName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentException(
                "Collection name must not be empty.",
                nameof(collectionName)
            );

        collectionNames[type] = collectionName;
    }

    public void RegisterMigrationTypes(Type baseType, Type migrationType)
    {
        migrationToBaseTypes[migrationType] = baseType;
    }

    public string GetCollectionName(Type type)
    {
        if (!collectionNames.ContainsKey(type))
            type = migrationToBaseTypes.GetValueOrDefault(type, type);
        
        if (!collectionNames.TryGetValue(type, out var name))
            throw new InvalidOperationException(
                $"No collection name registered for type '{type.FullName}'"
            );

        return name;
    }

    public IReadOnlyCollection<(Type Type, string CollectionName)> GetAllRegistered()
    {
        return collectionNames.Select(kvp => (kvp.Key, kvp.Value)).ToList();
    }
}