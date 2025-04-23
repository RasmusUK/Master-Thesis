using EventSource.Core;

namespace EventSource.Application.Interfaces;

public interface IGlobalReplayContext
{
    ReplayMode ReplayMode { get; }
    bool IsReplaying { get; }
    Guid? ReplayId { get; }
    void StartReplay(ReplayMode mode = ReplayMode.Strict);
    void StopReplay();
    IReadOnlyCollection<Event> GetEvents();
    void AddEvent(Event e);
}
