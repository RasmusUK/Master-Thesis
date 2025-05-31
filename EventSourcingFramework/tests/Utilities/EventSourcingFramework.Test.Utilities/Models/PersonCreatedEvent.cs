using EventSourcingFramework.Infrastructure.Shared.Models.Events;

namespace EventSourcingFramework.Test.Utilities.Models;

public record PersonCreatedEvent(PersonEntity Entity) : CreateEvent<PersonEntity>(Entity);