using EventSourcingFramework.Core;
using EventSourcingFramework.Core.Models.Events;

namespace EventSourcingFramework.Application.Abstractions;

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
