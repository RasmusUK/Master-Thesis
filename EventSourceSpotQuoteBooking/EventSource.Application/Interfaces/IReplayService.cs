using EventSource.Core;

namespace EventSource.Application.Interfaces;

public interface IReplayService
{
    Task ReplayAllEventsAsync();
    Task ReplayAllEventsUntilAsync(DateTime until);
    Task ReplayAllEventsFromUntilAsync(DateTime from, DateTime until);
    Task ReplayAllEntityEventsAsync(Guid entityId);
    Task ReplayAllEntityEventsUntilAsync(Guid entityId, DateTime until);
    Task ReplayAllEntityEventsFromUntilAsync(Guid entityId, DateTime from, DateTime until);
    Task ReplayEventsAsync(IReadOnlyCollection<Event> events);
    Task ReplayEventAsync(Event e);
}
