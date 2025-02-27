using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace EventSource.Persistence.Entities;

public abstract class MongoDbEntity<T>
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("objectType")]
    public string ObjectType { get; set; }

    [BsonElement("objectData")]
    public string ObjectData { get; set; }

    protected MongoDbEntity(Guid id, string objectType, string objectData)
    {
        Id = id;
        ObjectType = objectType;
        ObjectData = objectData;
    }

    public T ToDomain()
    {
        var type = Type.GetType(ObjectType);
        if (type is null)
            throw new InvalidOperationException($"Could not find type {type}");

        var deserialized = JsonConvert.DeserializeObject(ObjectData, type);
        if (deserialized is null)
            throw new InvalidOperationException($"Could not deserialize data for type {type}");

        return (T)deserialized;
    }

    public TE Deserialize<TE>()
        where TE : T
    {
        var deserialized = JsonConvert.DeserializeObject(ObjectData, typeof(TE));
        if (deserialized is null)
            throw new InvalidOperationException(
                $"Could not deserialize data for type {typeof(TE)}"
            );

        return (TE)deserialized;
    }
}
