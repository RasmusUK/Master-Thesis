namespace EventSource.Core.Interfaces;

public interface IEventProcessor
{
    Task<Entity> ProcessAsync(Event e);
    Task<Entity> ProcessHistoryAsync(Event e);
    void RegisterEventToEntity<TEvent, TEntity>()
        where TEvent : Event
        where TEntity : Entity;

    void RegisterEventHandler<TEvent>(IEventHandler<TEvent> eventHandler)
        where TEvent : Event;
}
