using EventSource.Persistence.Interfaces;

namespace EventSource.Persistence;

public class EntityCollectionNameProvider : IEntityCollectionNameProvider
{
    private readonly Dictionary<Type, string> collectionNames = new();

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

    public string GetCollectionName(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (!collectionNames.TryGetValue(type, out var name))
            throw new InvalidOperationException(
                $"No collection name registered for type '{type.FullName}'"
            );

        return name;
    }
}
