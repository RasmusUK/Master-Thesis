using System.Text.RegularExpressions;
using EventSource.Core.Events;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace EventSource.Persistence.Entities;

public abstract class MongoDbBase<T>
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement(nameof(ObjectType))]
    public string ObjectType { get; set; }

    [BsonElement(nameof(ObjectData))]
    public string ObjectData { get; set; }

    protected MongoDbBase(Guid id, object obj)
    {
        Id = id;
        ObjectType = obj.GetType().ToString();
        ObjectData = JsonConvert.SerializeObject(obj);
    }

    public T ToDomain()
    {
        var type =
            Type.GetType(ObjectType)
            ?? GetTypeFromAssemblies(ObjectType)
            ?? ResolveKnownGenericEventType(ObjectType);
        if (type is null)
            throw new InvalidOperationException($"Could not find type '{ObjectType}'");

        var deserialized = JsonConvert.DeserializeObject(ObjectData, type);
        if (deserialized is null)
            throw new InvalidOperationException($"Could not deserialize data for type '{type}'");

        return (T)deserialized;
    }

    public TE Deserialize<TE>()
        where TE : T
    {
        var deserialized = JsonConvert.DeserializeObject(ObjectData, typeof(TE));
        if (deserialized is null)
            throw new InvalidOperationException(
                $"Could not deserialize data for type '{typeof(TE)}' as '{ObjectType}'"
            );

        return (TE)deserialized;
    }

    private static Type? GetTypeFromAssemblies(string typeName) =>
        AppDomain
            .CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(typeName, false))
            .FirstOrDefault(t => t is not null);

    private static Type? ResolveKnownGenericEventType(string typeName)
    {
        var match = Regex.Match(typeName, @"^(.*)\`1\[(.*)\]$");
        if (!match.Success)
            return null;

        var baseTypeName = match.Groups[1].Value.Trim();
        var argTypeName = match.Groups[2].Value.Trim();

        var argType = GetTypeFromAssemblies(argTypeName);
        if (argType is null)
            return null;

        var openGenericType = KnownGenericEventTypes.FirstOrDefault(t =>
            t.FullName == baseTypeName
        );

        return openGenericType?.MakeGenericType(argType);
    }

    private static readonly Type[] KnownGenericEventTypes =
    {
        typeof(CreateEvent<>),
        typeof(UpdateEvent<>),
        typeof(DeleteEvent<>),
    };
}
