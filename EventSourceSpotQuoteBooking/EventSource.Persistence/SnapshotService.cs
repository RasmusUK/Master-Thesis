using System.Reflection;
using EventSource.Persistence.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventSource.Persistence;

public class SnapshotService : ISnapshotService
{
    private const string SnapshotMetadataCollection = "snapshots";

    private readonly IMongoDbService mongoDbService;
    private readonly IMongoDatabase entityDatabase;
    private readonly IEventSequenceGenerator eventSequenceGenerator;
    private readonly IEntityCollectionNameProvider entityCollectionNameProvider;

    public SnapshotService(
        IMongoDbService mongoDbService,
        IEventSequenceGenerator eventSequenceGenerator,
        IEntityCollectionNameProvider entityCollectionNameProvider
    )
    {
        this.mongoDbService = mongoDbService;
        this.eventSequenceGenerator = eventSequenceGenerator;
        this.entityCollectionNameProvider = entityCollectionNameProvider;
        entityDatabase = mongoDbService.GetEntityDatabase();
    }

    public async Task<string> TakeSnapshotAsync()
    {
        var dateTime = DateTime.UtcNow;
        var eventNumber = await eventSequenceGenerator.GetCurrentSequenceNumberAsync();
        var snapshotId = $"snapshot_{dateTime:yyyyMMddHHmmss}_{eventNumber}";

        var registered = entityCollectionNameProvider.GetAllRegistered();

        foreach (var (type, collectionName) in registered)
        {
            var method = typeof(SnapshotService).GetMethod(
                nameof(CopyCollectionAsync),
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            var genericMethod = method!.MakeGenericMethod(type);
            await (Task)genericMethod.Invoke(this, new object[] { collectionName, snapshotId })!;
        }

        var snapshotMeta = new BsonDocument
        {
            { "SnapshotId", snapshotId },
            { "EventNumber", eventNumber },
            { "Timestamp", dateTime },
        };

        await entityDatabase
            .GetCollection<BsonDocument>(SnapshotMetadataCollection)
            .InsertOneAsync(snapshotMeta);

        return snapshotId;
    }

    public async Task RestoreSnapshotAsync(string snapshotId)
    {
        var registered = entityCollectionNameProvider.GetAllRegistered();

        foreach (var (_, name) in registered)
        {
            await entityDatabase.DropCollectionAsync(name);
        }

        foreach (var (type, name) in registered)
        {
            var method = typeof(SnapshotService).GetMethod(
                nameof(RestoreCollectionAsync),
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            var genericMethod = method!.MakeGenericMethod(type);

            await (Task)genericMethod.Invoke(this, new object[] { name, snapshotId })!;
        }
    }

    public async Task<string?> GetLastSnapshotIdAsync()
    {
        var snapshots = await entityDatabase
            .GetCollection<BsonDocument>(SnapshotMetadataCollection)
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Sort(Builders<BsonDocument>.Sort.Descending("Timestamp"))
            .FirstOrDefaultAsync();

        return snapshots?["SnapshotId"]?.AsString;
    }

    public async Task<string?> GetLatestSnapshotBeforeAsync(long eventNumber)
    {
        var snapshot = await entityDatabase
            .GetCollection<BsonDocument>(SnapshotMetadataCollection)
            .Find(Builders<BsonDocument>.Filter.Lte("EventNumber", eventNumber))
            .Sort(Builders<BsonDocument>.Sort.Descending("EventNumber"))
            .FirstOrDefaultAsync();

        return snapshot?["SnapshotId"]?.AsString;
    }

    public async Task<string?> GetLatestSnapshotBeforeAsync(DateTime timestamp)
    {
        var snapshot = await entityDatabase
            .GetCollection<BsonDocument>(SnapshotMetadataCollection)
            .Find(Builders<BsonDocument>.Filter.Lte("Timestamp", timestamp))
            .Sort(Builders<BsonDocument>.Sort.Descending("Timestamp"))
            .FirstOrDefaultAsync();

        return snapshot?["SnapshotId"]?.AsString;
    }

    public async Task<IReadOnlyCollection<SnapshotMetadata>> GetAllSnapshotsAsync()
    {
        var database = mongoDbService.GetEntityDatabase();
        var collections = await (await database.ListCollectionNamesAsync()).ToListAsync();

        return collections
            .Where(c => c.Contains("_snapshot_"))
            .Select(c =>
            {
                var parts = c.Split('_');
                var eventNumber = long.TryParse(parts.ElementAtOrDefault(3), out var ev) ? ev : 0;
                var timestamp = DateTime.TryParseExact(
                    parts.ElementAtOrDefault(2),
                    "yyyyMMddHHmmss",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out var dt
                )
                    ? dt
                    : DateTime.MinValue;
                return new SnapshotMetadata
                {
                    SnapshotId = string.Join("_", parts.TakeLast(3)),
                    EventNumber = eventNumber,
                    Timestamp = timestamp,
                };
            })
            .GroupBy(s => s.SnapshotId)
            .Select(g => g.First())
            .OrderByDescending(x => x.Timestamp)
            .ToList();
    }

    public async Task DeleteSnapshotAsync(string snapshotId)
    {
        var database = mongoDbService.GetEntityDatabase();
        var collections = await (await database.ListCollectionNamesAsync()).ToListAsync();

        foreach (
            var collectionName in collections.Where(collectionName =>
                collectionName.EndsWith(snapshotId)
            )
        )
        {
            await database.DropCollectionAsync(collectionName);
        }
    }

    private async Task CopyCollectionAsync<T>(string originalCollectionName, string snapshotId)
    {
        var source = mongoDbService.GetCollection<T>(originalCollectionName);
        var snapshotName = $"{originalCollectionName}_{snapshotId}";
        var target = mongoDbService.GetCollection<T>(snapshotName);

        var allDocs = await source.Find(_ => true).ToListAsync();
        if (allDocs.Any())
            await target.InsertManyAsync(allDocs);
    }

    private async Task RestoreCollectionAsync<T>(string originalCollectionName, string snapshotId)
    {
        var snapshotName = $"{originalCollectionName}_{snapshotId}";
        var source = mongoDbService.GetCollection<T>(snapshotName);
        var target = mongoDbService.GetCollection<T>(originalCollectionName);

        var docs = await source.Find(_ => true).ToListAsync();
        if (docs.Any())
        {
            await mongoDbService.GetEntityDatabase().DropCollectionAsync(originalCollectionName);
            await target.InsertManyAsync(docs);
        }
    }
}
