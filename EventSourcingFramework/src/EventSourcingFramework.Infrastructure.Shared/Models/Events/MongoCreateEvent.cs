using EventSourcingFramework.Core.Models.Entity;
using EventSourcingFramework.Core.Models.Events;
using Newtonsoft.Json;

namespace EventSourcingFramework.Infrastructure.Shared.Models.Events;

public record CreateEvent<T>(T Entity) : MongoEventBase(Entity.Id), ICreateEvent<T>
    where T : IEntity
{
    public override string ToString()
    {
        return JsonConvert.SerializeObject(Entity, Formatting.Indented);
    }
}