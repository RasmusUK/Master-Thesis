namespace EventSource.Core.Events;

public record DeleteEvent<T> : Event
    where T : Entity
{
    public T Entity { get; init; }

    public DeleteEvent(T entity)
        : base(entity.Id)
    {
        Entity = entity;
    }
}
