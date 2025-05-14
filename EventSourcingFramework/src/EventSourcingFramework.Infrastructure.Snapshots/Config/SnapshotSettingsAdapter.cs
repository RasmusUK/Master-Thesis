using EventSourcingFramework.Application.Abstractions.Snapshots;
using EventSourcingFramework.Infrastructure.Shared.Configuration.Options;
using Microsoft.Extensions.Options;

namespace EventSourcingFramework.Infrastructure.Snapshots.Config;

public class SnapshotSettingsAdapter : ISnapshotSettings
{
    private readonly SnapshotOptions options;

    public SnapshotSettingsAdapter(IOptions<EventSourcingOptions> options)
    {
        this.options = options.Value.Snapshot;
    }

    public bool Enabled => options.Enabled;
    public SnapshotTriggerMode Mode => options.Trigger.Mode;
    public SnapshotFrequency Frequency => options.Trigger.Frequency;
    public int EventThreshold => options.Trigger.EventThreshold;
    public SnapshotRetentionStrategy Strategy => options.Retention.Strategy;
    public int MaxCount => options.Retention.MaxCount;
    public int MaxAgeDays => options.Retention.MaxAgeDays;
}