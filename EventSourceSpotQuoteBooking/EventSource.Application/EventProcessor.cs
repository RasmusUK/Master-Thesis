using EventSource.Core;
using EventSource.Core.Interfaces;

namespace EventSource.Application;

public class EventProcessor : IEventProcessor
{
    private readonly IEventStore eventStore;
    private readonly IEventHandler eventHandler;
    private readonly Dictionary<Type, Type> handlers = new();

    public EventProcessor(IEventStore eventStore, IEventHandler eventHandler)
    {
        this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        this.eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));
    }

    public void RegisterHandler<TEvent, TAggregateRoot>()
        where TEvent : Event
        where TAggregateRoot : AggregateRoot =>
        handlers.Add(typeof(TEvent), typeof(TAggregateRoot));

    public async Task ProcessAsync(Event e)
    {
        await eventStore.SaveEventAsync(e);
        var type = handlers[e.GetType()];
        var x = typeof(EventHandler).GetMethod(nameof(EventHandler.HandleAsync));
        var y = x.MakeGenericMethod(type);
        y.Invoke(eventHandler, new object[] { e });
    }
}
