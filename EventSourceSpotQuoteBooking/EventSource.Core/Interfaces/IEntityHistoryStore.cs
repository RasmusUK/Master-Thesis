namespace EventSource.Core.Interfaces;

public interface IEntityHistoryStore
{
    Task<IReadOnlyCollection<T>> GetEntityHistoryAsync<T>(Guid id)
        where T : Entity;
}
