using EventSourcingFramework.Application.Abstractions;
using EventSourcingFramework.Application.Abstractions.Snapshots;

namespace EventSourcing.Framework.Infrastructure.Shared.Configuration.Options;

public class SnapshotOptions
{
    public bool Enabled { get; set; } = true;

    public SnapshotTriggerOptions Trigger { get; set; } = new();

    public SnapshotRetentionOptions Retention { get; set; } = new();
}

public class SnapshotTriggerOptions
{
    public SnapshotTriggerMode Mode { get; set; } = SnapshotTriggerMode.Either;

    public SnapshotFrequency Frequency { get; set; } = SnapshotFrequency.Week;

    public int EventThreshold { get; set; } = 1000;
}

public class SnapshotRetentionOptions
{
    public SnapshotRetentionStrategy Strategy { get; set; } = SnapshotRetentionStrategy.Count;

    public int MaxCount { get; set; } = 20;

    public int MaxAgeDays { get; set; } = 180;
}
