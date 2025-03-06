namespace EventSource.Persistence;

public class MongoDbOptions
{
    public const string MongoDb = "MongoDb";
    public EventStoreOptions EventStore { get; set; } = new();
}

public class EventStoreOptions
{
    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
}