namespace EventSource.Core.Events;

public record UpdateEvent<T>(T Entity) : RepoEvent<T>(Entity)
    where T : Entity;
