namespace EventSourcingFramework.Persistence.Interfaces;

public interface IEntityCollectionNameProvider
{
    void Register(Type type, string collectionName);
    string GetCollectionName(Type type);
    IReadOnlyCollection<(Type Type, string CollectionName)> GetAllRegistered();
}
