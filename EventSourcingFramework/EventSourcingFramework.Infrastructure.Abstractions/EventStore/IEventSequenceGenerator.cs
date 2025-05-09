namespace EventSourcingFramework.Infrastructure.Abstractions.EventStore;

public interface IEventSequenceGenerator
{
    Task<long> GetNextSequenceNumberAsync();
    Task<long> GetCurrentSequenceNumberAsync();
}
