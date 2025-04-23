using EventSource.Core;

namespace EventSource.Application.Interfaces;

public interface IReplayService
{
    void StartReplay(ReplayMode mode = ReplayMode.Strict);
    Task StopReplay();
    Task ReplayAllAsync(bool autoStop = true);
    Task ReplayUntilAsync(DateTime until, bool autoStop = true);
    Task ReplayFromAsync(DateTime from, bool autoStop = true);
    Task ReplayFromUntilAsync(DateTime from, DateTime until, bool autoStop = true);

    Task ReplayEntityAsync(Guid entityId, bool autoStop = true);
    Task ReplayEntityUntilAsync(Guid entityId, DateTime until, bool autoStop = true);
    Task ReplayEntityFromUntilAsync(
        Guid entityId,
        DateTime from,
        DateTime until,
        bool autoStop = true
    );
    Task ReplayEventsAsync(IReadOnlyCollection<Event> events, bool autoStop = true);
    Task ReplayEventAsync(Event e, bool autoStop = true);
    IReadOnlyCollection<Event> GetSimulatedEvents();
    Task ReplayFromEventNumberAsync(long fromEventNumber, bool autoStop = true);
    Task ReplayUntilEventNumberAsync(long untilEventNumber, bool autoStop = true);
    Task ReplayFromUntilEventNumberAsync(
        long fromEventNumber,
        long untilEventNumber,
        bool autoStop = true
    );
    bool IsRunning();
}
