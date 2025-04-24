namespace EventSource.Persistence.Interfaces;

public interface IEventSequenceGenerator
{
    Task<long> GetNextSequenceNumberAsync();
    Task<long> GetCurrentSequenceNumberAsync();
}
