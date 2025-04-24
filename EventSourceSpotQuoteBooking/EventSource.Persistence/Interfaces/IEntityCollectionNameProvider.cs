namespace EventSource.Persistence.Interfaces;

public interface IEntityCollectionNameProvider
{
    void Register(Type type, string collectionName);
    string GetCollectionName(Type type);
}
