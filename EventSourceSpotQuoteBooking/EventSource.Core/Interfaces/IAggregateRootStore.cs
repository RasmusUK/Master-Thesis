namespace EventSource.Core.Interfaces;

public interface IAggregateRootStore
{
    Task SaveAggregateRootAsync(AggregateRoot aggregateRoot);
    Task<T?> GetAggregateRootAsync<T>(Guid id)
        where T : AggregateRoot;
    Task<IReadOnlyCollection<AggregateRoot>> GetAggregateRootsAsync();
}
