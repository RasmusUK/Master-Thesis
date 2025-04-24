using EventSource.Core;
using EventSource.Core.Events;
using Newtonsoft.Json;

namespace EventSource.Persistence.Events;

public record UpdateEvent<T>(T Entity) : EventBase(Entity.Id), IUpdateEvent<T>
    where T : IEntity
{
    public override string ToString() => JsonConvert.SerializeObject(Entity, Formatting.Indented);
}
