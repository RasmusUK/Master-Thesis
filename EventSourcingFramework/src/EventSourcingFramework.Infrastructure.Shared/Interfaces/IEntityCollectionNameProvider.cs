namespace EventSourcingFramework.Infrastructure.Shared.Interfaces;

public interface IEntityCollectionNameProvider
{
    void Register(Type type, string collectionName);
    void RegisterMigrationTypes(Type baseType, Type migrationType);
    string GetCollectionName(Type type);
    IReadOnlyCollection<(Type Type, string CollectionName)> GetAllRegistered();
}