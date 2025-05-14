using EventSourcingFramework.Core.Models.Entity;
using EventSourcingFramework.Core.Models.Events;
using Newtonsoft.Json;

namespace EventSourcingFramework.Infrastructure.Shared.Models.Events;

public record MongoDeleteEvent<T>(T Entity) : MongoEventBase(Entity.Id), IMongoDeleteEvent<T>
    where T : IEntity
{
    public override string ToString() => JsonConvert.SerializeObject(Entity, Formatting.Indented);
}
