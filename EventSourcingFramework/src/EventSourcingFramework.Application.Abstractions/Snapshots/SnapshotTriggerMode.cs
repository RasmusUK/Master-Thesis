namespace EventSourcingFramework.Application.Abstractions.Snapshots;

public enum SnapshotTriggerMode
{
    Time,
    EventCount,
    Either,
    Both
}