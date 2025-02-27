namespace EventSource.Core.Interfaces;

public interface IAggregateRootStore
{
    Task SaveAggregateRootAsync(AggregateRoot aggregateRoot);
    Task<IReadOnlyCollection<AggregateRoot>> GetAggregateRootsAsync();
}