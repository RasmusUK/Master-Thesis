using EventSourcingFramework.Application.Abstractions.Migrations;

namespace EventSourcingFramework.Infrastructure.Migrations.Services;

public class MigrationTypeRegistry : IMigrationTypeRegistry
{
    private readonly Dictionary<(Type targetType, int version), Type> versionMap = new();

    public void Register<TEntity>(int version, Type versionedType)
    {
        versionMap[(typeof(TEntity), version)] = versionedType;
    }

    public Type GetVersionedType(Type targetType, int version) => versionMap[(targetType, version)];

    public IReadOnlyCollection<Type> GetTypeToVersionedTypes(Type targetType) => 
        versionMap
        .Where(kvp => kvp.Key.targetType == targetType)
        .Select(kvp => kvp.Value)
        .ToList();
}