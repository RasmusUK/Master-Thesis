using EventSourcingFramework.Core.Enums;
using EventSourcingFramework.Core.Models.Events;

namespace EventSourcingFramework.Application.Abstractions.Replay;

public interface IReplayService
{
    Task StartReplayAsync(ReplayMode mode = ReplayMode.Strict);
    Task StopReplayAsync();
    Task ReplayAllAsync(bool autoStop = true, bool useSnapshot = true);
    Task ReplayUntilAsync(DateTime until, bool autoStop = true, bool useSnapshot = true);
    Task ReplayFromAsync(DateTime from, bool autoStop = true, bool useSnapshot = true);

    Task ReplayFromUntilAsync(
        DateTime from,
        DateTime until,
        bool autoStop = true,
        bool useSnapshot = true
    );

    Task ReplayEntityAsync(Guid entityId, bool autoStop = true, bool useSnapshot = true);

    Task ReplayEntityUntilAsync(
        Guid entityId,
        DateTime until,
        bool autoStop = true,
        bool useSnapshot = true
    );

    Task ReplayEntityFromUntilAsync(
        Guid entityId,
        DateTime from,
        DateTime until,
        bool autoStop = true,
        bool useSnapshot = true
    );

    Task ReplayEventsAsync(IReadOnlyCollection<IEvent> events, bool autoStop = true);
    Task ReplayEventAsync(IEvent e, bool autoStop = true);

    IReadOnlyCollection<IEvent> GetSimulatedEvents();

    Task ReplayFromEventNumberAsync(
        long fromEventNumber,
        bool autoStop = true,
        bool useSnapshot = true
    );

    Task ReplayUntilEventNumberAsync(
        long untilEventNumber,
        bool autoStop = true,
        bool useSnapshot = true
    );

    Task ReplayFromUntilEventNumberAsync(
        long fromEventNumber,
        long untilEventNumber,
        bool autoStop = true,
        bool useSnapshot = true
    );

    bool IsRunning();
}