namespace EventSource.Core.Events;

public record RepoEvent<T>(T Entity) : Event(Entity.Id)
    where T : Entity;
