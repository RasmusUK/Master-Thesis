using EventSource.Core.Events;

namespace EventSource.Application.Interfaces;

public interface IPersonalDataService
{
    Task StripAndStoreAsync(IEvent e);
    Task RestoreAsync(IEvent e);
}
