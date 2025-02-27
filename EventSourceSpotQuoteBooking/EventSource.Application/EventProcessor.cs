using EventSource.Core;
using EventSource.Core.Interfaces;

namespace EventSource.Application;

public class EventProcessor : IEventProcessor
{
    private readonly IEventStore eventStore;
    private readonly IServiceProvider serviceProvider;

    public EventProcessor(IEventStore eventStore, IServiceProvider serviceProvider)
    {
        this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        this.serviceProvider =
            serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task ProcessAsync(Event e)
    {
        await eventStore.SaveEventAsync(e);
        await DispatchToHandler(e);
    }

    private async Task DispatchToHandler(Event e)
    {
        var handlerType = typeof(IEventHandler<>).MakeGenericType(e.GetType());
        var handler = serviceProvider.GetService(handlerType);

        if (handler is null)
            throw new InvalidOperationException($"No handler found for event {e.GetType().Name}");

        await ((dynamic)handler).HandleEventAsync((dynamic)e);
    }
}
