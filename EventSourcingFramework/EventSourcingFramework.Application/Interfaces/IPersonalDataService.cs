using EventSourcingFramework.Core.Events;

namespace EventSourcingFramework.Application.Interfaces;

public interface IPersonalDataService
{
    Task StripAndStoreAsync(IEvent e);
    Task RestoreAsync(IEvent e);
}
