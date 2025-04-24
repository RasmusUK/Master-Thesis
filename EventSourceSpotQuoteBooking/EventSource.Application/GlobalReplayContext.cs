using EventSource.Application.Interfaces;
using EventSource.Core;
using EventSource.Core.Events;

namespace EventSource.Application;

public class GlobalReplayContext : IGlobalReplayContext
{
    private static readonly object LockObj = new();

    private bool isReplaying;
    private Guid? replayId;
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

    public Guid? ReplayId
    {
        get
        {
            lock (LockObj)
                return replayId;
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
            replayId = Guid.NewGuid();
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
            replayId = null;
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
