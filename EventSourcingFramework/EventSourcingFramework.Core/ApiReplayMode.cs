namespace EventSourcingFramework.Infrastructure.Http;

public enum ApiReplayMode
{
    CacheOnly,
    ExternalOnly,
    CacheThenExternal,
}