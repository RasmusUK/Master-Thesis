using EventSourcingFramework.Core.Models.Events;

namespace EventSourcingFramework.Application.Abstractions;

public interface IPersonalDataService
{
    Task StripAndStoreAsync(IEvent e);
    Task RestoreAsync(IEvent e);
}
