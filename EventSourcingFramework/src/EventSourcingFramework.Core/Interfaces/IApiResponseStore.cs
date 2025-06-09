namespace EventSourcingFramework.Core.Interfaces;

public interface IApiResponseStore
{
    Task<T?> GetAsync<T>(string key, long eventNumber);
    Task SaveAsync<T>(string key, long eventNumber, T response);
}