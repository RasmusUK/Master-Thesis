using EventSource.Core;
using EventSource.Core.Interfaces;

namespace EventSource.Application;

public abstract class EventHandler : IEventHandler<Event>
{
    public abstract Task HandleAsync(Event e);
}
