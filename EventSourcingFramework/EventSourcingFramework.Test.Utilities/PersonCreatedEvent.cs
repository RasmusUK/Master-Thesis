using EventSourcingFramework.Persistence.Events;

namespace EventSourcingFramework.Test.Utilities;

public record PersonCreatedEvent(PersonEntity Entity) : CreateEvent<PersonEntity>(Entity);
