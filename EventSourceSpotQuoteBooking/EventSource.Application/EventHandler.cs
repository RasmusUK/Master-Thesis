using EventSource.Core;
using EventSource.Core.Interfaces;

namespace EventSource.Application;

public class EventHandler : IEventHandler<Event>
{
    public Task HandleAsync(Event e)
    {
        throw new NotSupportedException("This handler should never be called");
    }
}
