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
        if (x is null)
            throw new InvalidOperationException(
                $"Could not find a method named {nameof(EventHandler.HandleAsync)}"
            );
        var y = x.MakeGenericMethod(type);
        var task = y.Invoke(eventHandler, new object[] { e });
        if (task is null)
            throw new InvalidOperationException(
                $"Could not invoke {nameof(EventHandler.HandleAsync)}"
            );
        await (Task)task;
    }
}
