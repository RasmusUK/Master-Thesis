namespace EventSource.Core.Events;

public record DeleteEvent<T>(T Entity) : RepoEvent<T>(Entity)
    where T : Entity;
