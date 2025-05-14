using EventSourcingFramework.Core.Models.Entity;
using EventSourcingFramework.Core.Models.Events;

namespace EventSourcingFramework.Application.Abstractions.EntityHistory;

public interface IEntityHistoryService
{
    Task<IReadOnlyCollection<T>> GetEntityHistoryAsync<T>(Guid id)
        where T : IEntity;

    Task<IReadOnlyCollection<(T entity, IEvent e)>> GetEntityHistoryWithEventsAsync<T>(Guid id)
        where T : IEntity;
}