using EventSource.Core;
using EventSource.Core.Events;
using Newtonsoft.Json;

namespace EventSource.Persistence.Events;

public record CreateEvent<T>(T Entity) : EventBase(Entity.Id), ICreateEvent<T>
    where T : IEntity
{
    public override string ToString() => JsonConvert.SerializeObject(Entity, Formatting.Indented);
}
