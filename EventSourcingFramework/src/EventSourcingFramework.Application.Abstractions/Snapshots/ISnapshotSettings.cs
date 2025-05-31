namespace EventSourcingFramework.Application.Abstractions.Snapshots;

public interface ISnapshotSettings
{
    bool Enabled { get; }
    SnapshotTriggerMode Mode { get; }
    SnapshotFrequency Frequency { get; }
    SnapshotRetentionStrategy Strategy { get; }
    int MaxCount { get; }
    int MaxAgeDays { get; }
}