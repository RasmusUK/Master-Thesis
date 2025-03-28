namespace EventSource.Core.Events;

public record CreateEvent<T> : Event
    where T : Entity
{
    public T Entity { get; init; }

    public CreateEvent(T entity)
        : base(entity.Id)
    {
        Entity = entity;
    }
}
