using EventSource.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventSource.Persistence.Entities;

public class MongoDbPersonalData : MongoDbBase<PersonalData>
{
    [BsonElement(nameof(EventId))]
    [BsonRepresentation(BsonType.String)]
    public Guid EventId { get; set; }

    public MongoDbPersonalData(PersonalData personalData)
        : base(Guid.NewGuid(), personalData)
    {
        EventId = personalData.EventId;
    }
}
