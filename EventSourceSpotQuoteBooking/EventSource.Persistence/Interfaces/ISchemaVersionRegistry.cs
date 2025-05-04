namespace EventSource.Persistence.Interfaces;

public interface ISchemaVersionRegistry
{
    void Register(Type type, int version);
    int GetVersion(Type type);
    int GetVersion<T>();
}
