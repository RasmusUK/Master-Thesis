namespace EventSource.Core.Interfaces;

public interface IPersonalDataStore
{
    Task StoreAsync(Guid eventId, Dictionary<string, object?> data);
    Task<Dictionary<string, object?>> RetrieveAsync(Guid eventId);
}
