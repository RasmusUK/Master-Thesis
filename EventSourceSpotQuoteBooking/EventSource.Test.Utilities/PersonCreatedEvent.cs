using EventSource.Persistence.Events;

namespace EventSource.Test.Utilities;

public record PersonCreatedEvent(PersonEntity Entity) : CreateEvent<PersonEntity>(Entity);
