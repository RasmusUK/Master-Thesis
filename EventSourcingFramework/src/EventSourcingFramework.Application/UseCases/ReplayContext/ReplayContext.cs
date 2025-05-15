using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Core.Enums;
using EventSourcingFramework.Core.Models.Events;

namespace EventSourcingFramework.Application.UseCases.ReplayContext;

public class ReplayContext : IReplayContext
{
    private static readonly object LockObj = new();
    private readonly List<IEvent> events = new();
    private ApiReplayMode apiReplayMode = ApiReplayMode.CacheOnly;
    private bool isLoading;

    private bool isReplaying;
    private ReplayMode replayMode = ReplayMode.Strict;

    public bool IsReplaying
    {
        get
        {
            lock (LockObj)
            {
                return isReplaying;
            }
        }
    }

    public bool IsLoading
    {
        get
        {
            lock (LockObj)
            {
                return isLoading;
            }
        }
        set
        {
            lock (LockObj)
            {
                isLoading = value;
            }
        }
    }

    public ReplayMode ReplayMode
    {
        get
        {
            lock (LockObj)
            {
                return replayMode;
            }
        }
    }

    public ApiReplayMode ApiReplayMode
    {
        get
        {
            lock (LockObj)
            {
                return apiReplayMode;
            }
        }
    }

    public void StartReplay(ReplayMode mode = ReplayMode.Strict, ApiReplayMode apiMode = ApiReplayMode.CacheOnly)
    {
        lock (LockObj)
        {
            if (isReplaying)
                throw new InvalidOperationException("Replay is already in progress.");

            isReplaying = true;
            replayMode = mode;
            apiReplayMode = apiMode;
            events.Clear();
        }
    }

    public void StopReplay()
    {
        lock (LockObj)
        {
            if (!isReplaying)
                throw new InvalidOperationException("Replay is not in progress.");

            isReplaying = false;
            replayMode = ReplayMode.Strict;
            events.Clear();
        }
    }

    public IReadOnlyCollection<IEvent> GetEvents()
    {
        lock (LockObj)
        {
            if (!isReplaying)
                throw new InvalidOperationException("Replay is not in progress.");

            return events.AsReadOnly();
        }
    }

    public void AddEvent(IEvent e)
    {
        lock (LockObj)
        {
            if (!isReplaying)
                throw new InvalidOperationException("Replay is not in progress.");

            events.Add(e);
        }
    }
}