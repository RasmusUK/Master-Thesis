namespace EventSource.Core.Events;

public record UpdateEvent<T> : Event
    where T : Entity
{
    public T Entity { get; init; }

    public UpdateEvent(T entity)
        : base(entity.Id)
    {
        Entity = entity;
    }
}
