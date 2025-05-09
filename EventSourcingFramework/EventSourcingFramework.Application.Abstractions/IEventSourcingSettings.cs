namespace EventSourcingFramework.Application.Abstractions;

public interface IEventSourcingSettings
{
    public bool EnablePersonalDataStore { get; }
    public bool EnableEventStore { get; }
    public bool EnableEntityStore { get; }
}