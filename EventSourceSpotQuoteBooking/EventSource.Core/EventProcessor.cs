namespace EventSource.Core;

public class EventProcessor : IEventProcessor
{
    private readonly IEventStore eventStore;
    private readonly IProjectionHandler projectionHandler;

    public EventProcessor(IEventStore eventStore, IProjectionHandler projectionHandler)
    {
        this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        this.projectionHandler = projectionHandler ?? throw new ArgumentNullException(nameof(projectionHandler));
    }

    public async Task ProcessAsync(Event e)
    {
        await eventStore.SaveEventAsync(e);
        await projectionHandler.HandleAsync(e);
    }
}