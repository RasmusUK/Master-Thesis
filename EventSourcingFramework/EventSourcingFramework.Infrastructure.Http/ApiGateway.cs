using System.Text;
using System.Text.Json;
using EventSourcingFramework.Application.Abstractions;
using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Core.Enums;
using EventSourcingFramework.Core.Interfaces;

namespace EventSourcingFramework.Infrastructure.Http;

public class ApiGateway : IApiGateway
{
    private readonly HttpClient httpClient;
    private readonly IApiResponseStore responseStore;
    private readonly IGlobalReplayContext replayContext;

    public ApiGateway(HttpClient httpClient, IApiResponseStore responseStore, IGlobalReplayContext replayContext)
    {
        this.httpClient = httpClient;
        this.responseStore = responseStore;
        this.replayContext = replayContext;
    }

    public async Task<T> GetAsync<T>(string url, Dictionary<string, string>? headers = null)
    {
        var key = $"GET:{url}";
        return await HandleRequest<T>(() => BuildRequest<>(HttpMethod.Get, url, null, headers), key);
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest body, Dictionary<string, string>? headers = null)
    {
        var key = $"POST:{url}:{JsonSerializer.Serialize(body)}";
        return await HandleRequest<TResponse>(() => BuildRequest(HttpMethod.Post, url, body, headers), key);
    }

    public async Task<TResponse> SendAsync<TResponse>(HttpRequestMessage request)
    {
        var content = request.Content is not null 
            ? await request.Content.ReadAsStringAsync() 
            : "";
        var key = $"{request.Method}:{request.RequestUri}:{content}";
        return await HandleRequest<TResponse>(() => Task.FromResult(request), key);
    }

    private async Task<T> HandleRequest<T>(Func<Task<HttpRequestMessage>> buildRequest, string key)
    {
        if (replayContext.IsReplaying)
        {
            switch (replayContext.ApiReplayMode)
            {
                case ApiReplayMode.CacheOnly:
                    {
                        var cached = await responseStore.GetAsync<T>(key);
                        if (cached is not null)
                            return cached;

                        throw new InvalidOperationException($"No cached response found for key '{key}' during replay with StorageOnly mode.");
                    }

                case ApiReplayMode.ExternalOnly:
                    {
                        var request = await buildRequest();
                        var response = await httpClient.SendAsync(request);
                        response.EnsureSuccessStatusCode();

                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<T>(json);

                        await responseStore.SaveAsync(key, result!);
                        return result!;
                    }

                case ApiReplayMode.CacheThenExternal:
                    {
                        var cached = await responseStore.GetAsync<T>(key);
                        if (cached is not null)
                            return cached;

                        var request = await buildRequest();
                        var response = await httpClient.SendAsync(request);
                        response.EnsureSuccessStatusCode();

                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<T>(json);

                        await responseStore.SaveAsync(key, result!);
                        return result!;
                    }

                default:
                    throw new InvalidOperationException($"Unsupported replay mode: {replayContext.ReplayMode}");
            }
        }

        var liveRequest = await buildRequest();
        var liveResponse = await httpClient.SendAsync(liveRequest);
        liveResponse.EnsureSuccessStatusCode();

        var liveJson = await liveResponse.Content.ReadAsStringAsync();
        var liveResult = JsonSerializer.Deserialize<T>(liveJson);

        await responseStore.SaveAsync(key, liveResult!);
        return liveResult!;
    }

    private Task<HttpRequestMessage> BuildRequest<T>(HttpMethod method, string url, T? body, Dictionary<string, string>? headers)
    {
        var request = new HttpRequestMessage(method, url);

        if (headers is not null)
            foreach (var kv in headers)
                request.Headers.Add(kv.Key, kv.Value);

        if (body == null) return Task.FromResult(request);
        
        var json = JsonSerializer.Serialize(body);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        return Task.FromResult(request);
    }
}
