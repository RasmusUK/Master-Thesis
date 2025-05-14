using EventSourcingFramework.Application.Abstractions.Migrations;

namespace EventSourcingFramework.Infrastructure.Migrations.Services;

public class SchemaVersionRegistry : ISchemaVersionRegistry
{
    private readonly Dictionary<Type, int> versions = new();

    public void Register(Type type, int version)
    {
        versions[type] = version;
    }

    public int GetVersion(Type type)
    {
        return versions.TryGetValue(type, out var v) ? v : 1;
    }

    public int GetVersion<T>() => GetVersion(typeof(T));
}
