using EventSourcingFramework.Application.Abstractions.Migrations;

namespace EventSourcingFramework.Infrastructure.Migrations.Services;

public class MigrationTypeRegistry : IMigrationTypeRegistry
{
    private readonly Dictionary<(Type targetType, int version), Type> versionMap = new();

    public void Register<TEntity>(int version, Type versionedType)
    {
        versionMap[(typeof(TEntity), version)] = versionedType;
    }

    public Type GetVersionedType(Type targetType, int version)
    {
        return versionMap[(targetType, version)];
    }

    public Type? GetBaseType(Type targetType)
    {
        var versionedTypes = versionMap
            .Where(kvp => kvp.Value == targetType)
            .Select(kvp => kvp.Key.targetType)
            .ToList();

        return versionedTypes.FirstOrDefault();
    }
}