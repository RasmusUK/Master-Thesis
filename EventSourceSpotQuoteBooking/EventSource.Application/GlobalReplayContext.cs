using EventSource.Application.Interfaces;
using EventSource.Core;
using EventSource.Core.Events;

namespace EventSource.Application;

public class GlobalReplayContext : IGlobalReplayContext
{
    private static readonly object LockObj = new();

    private bool isReplaying;
    private bool isLoading;
    private ReplayMode replayMode = ReplayMode.Strict;
    private readonly List<IEvent> events = new();

    public bool IsReplaying
    {
        get
        {
            lock (LockObj)
                return isReplaying;
        }
    }

    public bool IsLoading
    {
        get
        {
            lock (LockObj)
                return isLoading;
        }
        set
        {
            lock (LockObj)
                isLoading = value;
        }
    }

    public ReplayMode ReplayMode
    {
        get
        {
            lock (LockObj)
                return replayMode;
        }
    }

    public void StartReplay(ReplayMode mode = ReplayMode.Strict)
    {
        lock (LockObj)
        {
            if (isReplaying)
                throw new InvalidOperationException("Replay is already in progress.");

            isReplaying = true;
            replayMode = mode;
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
            if (!isReplaying || ReplayMode != ReplayMode.Sandbox)
                throw new InvalidOperationException("Replay is not in progress.");

            events.Add(e);
        }
    }
}
