using EventSourcingFramework.Core;
using EventSourcingFramework.Core.Enums;
using EventSourcingFramework.Core.Models.Events;

namespace EventSourcingFramework.Application.Abstractions.ReplayContext;

public interface IGlobalReplayContext
{
    ReplayMode ReplayMode { get; }
    ApiReplayMode ApiReplayMode { get; }
    bool IsReplaying { get; }
    bool IsLoading { get; set; }
    void StartReplay(ReplayMode mode = ReplayMode.Strict, ApiReplayMode apiMode = ApiReplayMode.CacheOnly);
    void StopReplay();
    IReadOnlyCollection<IEvent> GetEvents();
    void AddEvent(IEvent e);
}
