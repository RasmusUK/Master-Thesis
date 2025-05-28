namespace EventSourcingFramework.Application.Abstractions.Migrations;

public interface IMigrationTypeRegistry
{
    void Register<TEntity>(int version, Type versionedType);
    Type GetVersionedType(Type targetType, int version);
    IReadOnlyCollection<Type> GetTypeToVersionedTypes(Type targetType);
}