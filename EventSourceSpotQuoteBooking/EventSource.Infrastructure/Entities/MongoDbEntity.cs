using EventSource.Core;

namespace EventSource.Persistence.Entities;

public class MongoDbEntity : MongoDbBase<Entity>
{
    public MongoDbEntity(Entity entity)
        : base(entity.Id, entity) { }
}
