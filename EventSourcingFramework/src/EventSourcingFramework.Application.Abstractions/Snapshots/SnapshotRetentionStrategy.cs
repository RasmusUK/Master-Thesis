namespace EventSourcingFramework.Application.Abstractions.Snapshots;

public enum SnapshotRetentionStrategy
{
    All,
    Count,
    Time
}