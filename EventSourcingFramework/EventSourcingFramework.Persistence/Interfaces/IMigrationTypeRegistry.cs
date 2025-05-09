namespace EventSourcingFramework.Persistence.Interfaces;

public interface IMigrationTypeRegistry
{
    void Register<TEntity>(int version, Type versionedType);
    Type GetVersionedType(Type targetType, int version);
    Type? GetBaseType(Type targetType);
}
