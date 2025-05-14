namespace EventSourcingFramework.Application.Abstractions.EventSourcingSettings;

public interface IEventSourcingSettings
{
    public bool EnablePersonalDataStore { get; }
    public bool EnableEventStore { get; }
    public bool EnableEntityStore { get; }
}