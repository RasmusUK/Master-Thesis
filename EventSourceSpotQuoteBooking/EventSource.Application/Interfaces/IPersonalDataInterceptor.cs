using EventSource.Core;

namespace EventSource.Application.Interfaces;

public interface IPersonalDataInterceptor
{
    Task<Event> ProcessEventForStorage(Event e);
    Task<Event> ProcessEventForRetrieval(Event e);
}
