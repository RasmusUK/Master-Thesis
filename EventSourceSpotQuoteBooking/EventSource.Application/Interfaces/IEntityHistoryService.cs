using EventSource.Core;

namespace EventSource.Application.Interfaces;

public interface IEntityHistoryService
{
    Task<IReadOnlyCollection<T>> GetEntityHistoryAsync<T>(Guid id)
        where T : Entity;
    Task<IReadOnlyCollection<(T entity, Event e)>> GetEntityHistoryWithEventsAsync<T>(Guid id)
        where T : Entity;
}
