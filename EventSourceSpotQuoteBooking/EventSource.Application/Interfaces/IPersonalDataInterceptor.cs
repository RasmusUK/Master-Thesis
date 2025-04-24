using EventSource.Core.Events;

namespace EventSource.Application.Interfaces;

public interface IPersonalDataInterceptor
{
    Task<IEvent> ProcessEventForStorage(IEvent e);
    Task<IEvent> ProcessEventForRetrieval(IEvent e);
}
