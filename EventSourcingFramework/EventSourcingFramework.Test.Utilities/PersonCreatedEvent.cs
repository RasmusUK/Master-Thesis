using EventSourcing.Framework.Infrastructure.Shared.Models.Events;

namespace EventSourcingFramework.Test.Utilities;

public record PersonCreatedEvent(PersonEntity Entity) : MongoCreateEvent<PersonEntity>(Entity);
