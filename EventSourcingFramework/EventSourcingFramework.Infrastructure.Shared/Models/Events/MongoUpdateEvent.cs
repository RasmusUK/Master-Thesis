using EventSourcingFramework.Core;
using EventSourcingFramework.Core.Models;
using EventSourcingFramework.Core.Models.Entity;
using EventSourcingFramework.Core.Models.Events;
using Newtonsoft.Json;

namespace EventSourcing.Framework.Infrastructure.Shared.Models.Events;

public record MongoUpdateEvent<T>(T Entity) : MongoEventBase(Entity.Id), IMongoUpdateEvent<T>
    where T : IEntity
{
    public override string ToString() => JsonConvert.SerializeObject(Entity, Formatting.Indented);
}
