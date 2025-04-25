using System.Reflection;
using EventSource.Application;
using EventSource.Application.Interfaces;
using EventSource.Core;
using EventSource.Infrastructure.Interfaces;
using EventSource.Persistence.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventSource.Infrastructure;

public class SnapshotService : ISnapshotService
{
    private const string SnapshotMetadataCollection = "snapshots";

    private readonly IMongoDbService mongoDbService;
    private readonly IEventSequenceGenerator eventSequenceGenerator;
    private readonly IEntityCollectionNameProvider entityCollectionNameProvider;
    private readonly IGlobalReplayContext globalReplayContext;
    private static readonly SemaphoreSlim SnapshotLock = new(1, 1);

    public SnapshotService(
        IMongoDbService mongoDbService,
        IEventSequenceGenerator eventSequenceGenerator,
        IEntityCollectionNameProvider entityCollectionNameProvider,
        IGlobalReplayContext globalReplayContext
    )
    {
        this.mongoDbService = mongoDbService;
        this.eventSequenceGenerator = eventSequenceGenerator;
        this.entityCollectionNameProvider = entityCollectionNameProvider;
        this.globalReplayContext = globalReplayContext;
    }

    public async Task<string> TakeSnapshotAsync()
    {
        await SnapshotLock.WaitAsync();
        try
        {
            if (globalReplayContext is { IsReplaying: true, IsLoading: false })
                throw new InvalidOperationException(
                    "Cannot take a snapshot while in replay mode. Please stop the replay first."
                );

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
                await (Task)
                    genericMethod.Invoke(this, new object[] { collectionName, snapshotId })!;
            }

            var snapshotMeta = new BsonDocument
            {
                { "SnapshotId", snapshotId },
                { "EventNumber", eventNumber },
                { "Timestamp", dateTime },
            };

            await mongoDbService
                .GetEntityDatabase(true)
                .GetCollection<BsonDocument>(SnapshotMetadataCollection)
                .InsertOneAsync(snapshotMeta);

            return snapshotId;
        }
        finally
        {
            SnapshotLock.Release();
        }
    }

    public async Task RestoreSnapshotAsync(string snapshotId)
    {
        if (!globalReplayContext.IsReplaying)
        {
            globalReplayContext.StartReplay(ReplayMode.Debug);
            await mongoDbService.UseDebugEntityDatabase();
        }

        await SnapshotLock.WaitAsync();
        try
        {
            var registered = entityCollectionNameProvider.GetAllRegistered();
            var renamedBackups = new List<string>();

            try
            {
                if (globalReplayContext.ReplayMode != ReplayMode.Debug)
                {
                    foreach (var (_, originalName) in registered)
                    {
                        var backupName = $"{originalName}_backup";

                        var collections = await (
                            await mongoDbService.GetEntityDatabase(true).ListCollectionNamesAsync()
                        ).ToListAsync();
                        if (collections.Contains(backupName))
                            await mongoDbService
                                .GetEntityDatabase(true)
                                .DropCollectionAsync(backupName);

                        await mongoDbService
                            .GetEntityDatabase(true)
                            .RenameCollectionAsync(originalName, backupName);
                        renamedBackups.Add(originalName);
                    }
                }

                foreach (var (type, originalName) in registered)
                {
                    var method = typeof(SnapshotService).GetMethod(
                        nameof(RestoreCollectionAsync),
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );

                    var genericMethod = method!.MakeGenericMethod(type);
                    await (Task)
                        genericMethod.Invoke(this, new object[] { originalName, snapshotId })!;
                }

                if (globalReplayContext.ReplayMode != ReplayMode.Debug)
                {
                    foreach (var originalName in renamedBackups)
                    {
                        var backupName = $"{originalName}_backup";
                        var collections = await (
                            await mongoDbService.GetEntityDatabase(true).ListCollectionNamesAsync()
                        ).ToListAsync();
                        if (collections.Contains(backupName))
                            await mongoDbService
                                .GetEntityDatabase(true)
                                .DropCollectionAsync(backupName);
                    }
                }
            }
            catch (Exception ex)
            {
                if (globalReplayContext.ReplayMode != ReplayMode.Debug)
                {
                    try
                    {
                        foreach (var originalName in renamedBackups)
                        {
                            var backupName = $"{originalName}_backup";

                            var collections = await (
                                await mongoDbService
                                    .GetEntityDatabase(true)
                                    .ListCollectionNamesAsync()
                            ).ToListAsync();

                            if (collections.Contains(originalName))
                                await mongoDbService
                                    .GetEntityDatabase(true)
                                    .DropCollectionAsync(originalName);

                            if (collections.Contains(backupName))
                                await mongoDbService
                                    .GetEntityDatabase(true)
                                    .RenameCollectionAsync(backupName, originalName);
                        }
                    }
                    catch (Exception rollbackEx)
                    {
                        throw new AggregateException(
                            "Snapshot restore failed AND rollback failed.",
                            ex,
                            rollbackEx
                        );
                    }

                    throw new InvalidOperationException(
                        $"Snapshot restore failed, but rollback to previous state succeeded: {ex.Message}",
                        ex
                    );
                }
            }
        }
        finally
        {
            SnapshotLock.Release();
        }
    }

    public async Task<string?> GetLastSnapshotIdAsync()
    {
        var snapshots = await mongoDbService
            .GetEntityDatabase(true)
            .GetCollection<BsonDocument>(SnapshotMetadataCollection)
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Sort(Builders<BsonDocument>.Sort.Descending("Timestamp"))
            .FirstOrDefaultAsync();

        return snapshots?["SnapshotId"]?.AsString;
    }

    public async Task<string?> GetLatestSnapshotBeforeAsync(long eventNumber)
    {
        var snapshot = await mongoDbService
            .GetEntityDatabase(true)
            .GetCollection<BsonDocument>(SnapshotMetadataCollection)
            .Find(Builders<BsonDocument>.Filter.Lte("EventNumber", eventNumber))
            .Sort(Builders<BsonDocument>.Sort.Descending("EventNumber"))
            .FirstOrDefaultAsync();

        return snapshot?["SnapshotId"]?.AsString;
    }

    public async Task<string?> GetLatestSnapshotBeforeAsync(DateTime timestamp)
    {
        var snapshot = await mongoDbService
            .GetEntityDatabase(true)
            .GetCollection<BsonDocument>(SnapshotMetadataCollection)
            .Find(Builders<BsonDocument>.Filter.Lte("Timestamp", timestamp))
            .Sort(Builders<BsonDocument>.Sort.Descending("Timestamp"))
            .FirstOrDefaultAsync();

        return snapshot?["SnapshotId"]?.AsString;
    }

    public async Task<IReadOnlyCollection<SnapshotMetadata>> GetAllSnapshotsAsync()
    {
        var database = mongoDbService.GetEntityDatabase(true);
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

    public async Task<SnapshotMetadata?> GetLastSnapshotAsync() =>
        await mongoDbService
            .GetEntityDatabase(true)
            .GetCollection<SnapshotMetadata>(SnapshotMetadataCollection)
            .Find(FilterDefinition<SnapshotMetadata>.Empty)
            .Sort(Builders<SnapshotMetadata>.Sort.Descending("Timestamp"))
            .FirstOrDefaultAsync();

    public async Task DeleteSnapshotAsync(string snapshotId)
    {
        var database = mongoDbService.GetEntityDatabase(true);
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
        var source = mongoDbService.GetCollection<T>(originalCollectionName, true);
        var snapshotName = $"{originalCollectionName}_{snapshotId}";
        var target = mongoDbService.GetCollection<T>(snapshotName, true);

        var allDocs = await source.Find(_ => true).ToListAsync();
        await target.InsertManyAsync(allDocs);
    }

    private async Task RestoreCollectionAsync<T>(string originalCollectionName, string snapshotId)
    {
        var snapshotName = $"{originalCollectionName}_{snapshotId}";
        var source = mongoDbService.GetCollection<T>(snapshotName, true);
        var target = mongoDbService.GetCollection<T>(
            originalCollectionName,
            globalReplayContext.ReplayMode != ReplayMode.Debug
        );

        var docs = await source.Find(_ => true).ToListAsync();
        await mongoDbService
            .GetEntityDatabase(globalReplayContext.ReplayMode != ReplayMode.Debug)
            .DropCollectionAsync(originalCollectionName);
        await target.InsertManyAsync(docs);
    }
}
