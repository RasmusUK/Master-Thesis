using EventSource.Core;
using EventSource.Core.Events;

namespace EventSource.Application.Interfaces;

public interface IGlobalReplayContext
{
    ReplayMode ReplayMode { get; }
    bool IsReplaying { get; }
    Guid? ReplayId { get; }
    void StartReplay(ReplayMode mode = ReplayMode.Strict);
    void StopReplay();
    IReadOnlyCollection<IEvent> GetEvents();
    void AddEvent(IEvent e);
}
