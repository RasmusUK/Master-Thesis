namespace EventSource.Core;

public interface IProjectionHandler
{
    Task HandleAsync(Event e);
}