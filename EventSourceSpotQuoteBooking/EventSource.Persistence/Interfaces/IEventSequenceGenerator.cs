namespace EventSource.Persistence.Interfaces;

public interface IEventSequenceGenerator
{
    Task<long> GetNextSequenceNumberAsync();
}
