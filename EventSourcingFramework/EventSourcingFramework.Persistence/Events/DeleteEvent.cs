using EventSourcingFramework.Core;
using EventSourcingFramework.Core.Events;
using Newtonsoft.Json;

namespace EventSourcingFramework.Persistence.Events;

public record DeleteEvent<T>(T Entity) : EventBase(Entity.Id), IDeleteEvent<T>
    where T : IEntity
{
    public override string ToString() => JsonConvert.SerializeObject(Entity, Formatting.Indented);
}
