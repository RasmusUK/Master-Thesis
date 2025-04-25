using EventSource.Core.Events;

namespace EventSource.Application.Interfaces;

public interface IGlobalReplayContext
{
    ReplayMode ReplayMode { get; }
    bool IsReplaying { get; }
    bool IsLoading { get; set; }
    void StartReplay(ReplayMode mode = ReplayMode.Strict);
    void StopReplay();
    IReadOnlyCollection<IEvent> GetEvents();
    void AddEvent(IEvent e);
}
