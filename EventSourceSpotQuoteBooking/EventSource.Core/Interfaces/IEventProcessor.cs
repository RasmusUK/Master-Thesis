namespace EventSource.Core.Interfaces;

public interface IEventProcessor
{
    Task<Entity> ProcessAsync(Event e);
    Task<Entity> ProcessHistoryAsync(Event e);
    void RegisterHandler<TEvent, TEntity>()
        where TEvent : Event
        where TEntity : Entity;
}
