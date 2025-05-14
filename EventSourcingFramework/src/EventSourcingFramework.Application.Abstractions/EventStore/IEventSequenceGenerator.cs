namespace EventSourcingFramework.Application.Abstractions.EventStore;

public interface IEventSequenceGenerator
{
    Task<long> GetNextSequenceNumberAsync();
    Task<long> GetCurrentSequenceNumberAsync();
}