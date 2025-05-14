namespace EventSourcingFramework.Infrastructure.Abstractions.ApiResponseStore;

public interface IApiResponseStore
{
    Task<T?> GetAsync<T>(string key);
    Task SaveAsync<T>(string key, T response);
}