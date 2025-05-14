namespace EventSourcingFramework.Core.Enums;

public enum ApiReplayMode
{
    CacheOnly,
    ExternalOnly,
    CacheThenExternal
}