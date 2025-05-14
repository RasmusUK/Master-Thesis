namespace EventSourcingFramework.Infrastructure.Shared.Configuration.Options;

public class MongoDbOptions
{
    public const string MongoDb = "MongoDb";
    public DatabaseOptions EventStore { get; set; } = new();
    public DatabaseOptions EntityStore { get; set; } = new();
    public DatabaseOptions DebugEntityStore { get; set; } = new();
    public DatabaseOptions PersonalDataStore { get; set; } = new();
    public DatabaseOptions ApiResponseStore { get; set; } = new();
}

public class DatabaseOptions
{
    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
}
