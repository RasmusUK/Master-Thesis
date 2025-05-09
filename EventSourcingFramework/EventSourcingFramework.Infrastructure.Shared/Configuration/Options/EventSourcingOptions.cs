using EventSourcingFramework.Application.Abstractions;

namespace EventSourcing.Framework.Infrastructure.Shared.Configuration.Options;

public class EventSourcingOptions : IEventSourcingSettings
{
    public const string EventSourcing = "EventSourcing";
    public SnapshotOptions Snapshot { get; set; } = new();
    public bool EnablePersonalDataStore { get; set; } = true;
    public bool EnableEventStore { get; set; } = true;
    public bool EnableEntityStore { get; set; } = true;
}
