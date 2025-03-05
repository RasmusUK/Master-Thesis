namespace EventSource.Core.Interfaces;

public interface IEventHandler
{
    Task<TEntity> HandleAsync<TEntity>(Event e)
        where TEntity : Entity;
}
