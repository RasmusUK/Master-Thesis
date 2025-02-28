namespace EventSource.Core.Interfaces;

public interface IEventProcessor
{
    Task ProcessAsync(Event e);

    void RegisterHandler<TEvent, TAggregateRoot>()
        where TEvent : Event
        where TAggregateRoot : AggregateRoot;
}
