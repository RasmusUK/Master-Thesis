namespace EventSource.Core.Events;

public record CreateEvent<T>(T Entity) : RepoEvent<T>(Entity)
    where T : Entity;
