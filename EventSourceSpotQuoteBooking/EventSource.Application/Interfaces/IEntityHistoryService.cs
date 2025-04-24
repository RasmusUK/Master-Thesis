using EventSource.Core;
using EventSource.Core.Events;

namespace EventSource.Application.Interfaces;

public interface IEntityHistoryService
{
    Task<IReadOnlyCollection<T>> GetEntityHistoryAsync<T>(Guid id)
        where T : IEntity;
    Task<IReadOnlyCollection<(T entity, IEvent e)>> GetEntityHistoryWithEventsAsync<T>(Guid id)
        where T : IEntity;
}
