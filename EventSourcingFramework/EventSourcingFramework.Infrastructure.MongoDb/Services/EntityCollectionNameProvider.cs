using EventSourcing.Framework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Application.Abstractions.Migrations;

namespace EventSourcingFramework.Infrastructure.MongoDb.Services;

public class EntityCollectionNameProvider : IEntityCollectionNameProvider
{
    private readonly Dictionary<Type, string> collectionNames = new();
    private readonly IMigrationTypeRegistry migrationTypeRegistry;

    public EntityCollectionNameProvider(IMigrationTypeRegistry migrationTypeRegistry)
    {
        this.migrationTypeRegistry = migrationTypeRegistry;
    }

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
        if (!collectionNames.ContainsKey(type))
            type =
                migrationTypeRegistry.GetBaseType(type)
                ?? throw new InvalidOperationException(
                    $"No base type registered for '{type.FullName}'"
                );

        if (!collectionNames.TryGetValue(type, out var name))
            throw new InvalidOperationException(
                $"No collection name registered for type '{type.FullName}'"
            );

        return name;
    }

    public IReadOnlyCollection<(Type Type, string CollectionName)> GetAllRegistered() =>
        collectionNames.Select(kvp => (kvp.Key, kvp.Value)).ToList();
}
