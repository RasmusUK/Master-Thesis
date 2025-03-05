using EventSource.Core;
using EventSource.Core.Interfaces;

namespace EventSource.Application;

public class EventHandler : IEventHandler
{
    public async Task<TEntity> HandleAsync<TEntity>(Event e)
        where TEntity : Entity
    {
        throw new NotImplementedException();
    }
}
