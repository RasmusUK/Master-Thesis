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
        var type = Type.GetType(ObjectType) ?? GetTypeFromAssemblies(ObjectType);
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
}
