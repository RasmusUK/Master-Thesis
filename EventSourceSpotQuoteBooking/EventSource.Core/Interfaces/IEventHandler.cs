namespace EventSource.Core.Interfaces;

public interface IEventHandler
{
    Task HandleAsync<TAggregateRoot>(Event e)
        where TAggregateRoot : Entity;
}
