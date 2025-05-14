namespace EventSourcingFramework.Infrastructure.Http;

public interface IApiGateway
{
    Task<T> GetAsync<T>(string url, Dictionary<string, string>? headers = null);
    Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest body, Dictionary<string, string>? headers = null);
    Task<TResponse> SendAsync<TResponse>(HttpRequestMessage request);
}