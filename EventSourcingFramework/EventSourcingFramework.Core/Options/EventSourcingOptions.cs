namespace EventSourcingFramework.Core.Options;

public class EventSourcingOptions
{
    public const string EventSourcing = "EventSourcing";
    public SnapshotOptions Snapshot { get; set; } = new();
    public bool EnablePersonalDataStore { get; set; } = true;
    public bool EnableEventStore { get; set; } = true;
    public bool EnableEntityStore { get; set; } = true;
}
