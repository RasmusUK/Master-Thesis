using System.Reflection;
using EventSourcingFramework.Application.Abstractions.EventStore;
using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Application.Abstractions.Snapshots;
using EventSourcingFramework.Core.Enums;
using EventSourcingFramework.Infrastructure.Shared.Configuration.Options;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace EventSourcingFramework.Infrastructure.Snapshots.Services;

public class SnapshotService : ISnapshotService
{
    private const string SnapshotMetadataCollection = "snapshots";
    private static readonly SemaphoreSlim SnapshotLock = new(1, 1);
    private readonly IEntityCollectionNameProvider entityCollectionNameProvider;
    private readonly IReplayContext replayContext;

    private readonly IMongoDbService mongoDbService;
    private readonly object snapshotCacheLock = new();
    private readonly SnapshotOptions snapshotOptions;
    private SnapshotMetadata? lastSnapshotCached;

    public SnapshotService(
        IMongoDbService mongoDbService,
        IEntityCollectionNameProvider entityCollectionNameProvider,
        IReplayContext replayContext,
        IOptions<EventSourcingOptions> eventSourcingOptions
    )
    {
        this.mongoDbService = mongoDbService;
        this.entityCollectionNameProvider = entityCollectionNameProvider;
        this.replayContext = replayContext;
        snapshotOptions = eventSourcingOptions.Value.Snapshot;
    }

    public async Task TakeSnapshotIfNeededAsync(long currentEventNumber)
    {
        if (!snapshotOptions.Enabled || replayContext.IsReplaying)
            return;

        SnapshotMetadata? lastSnapshot;
        lock (snapshotCacheLock)
        {
            lastSnapshot = lastSnapshotCached;
        }

        if (lastSnapshot == null)
        {
            lastSnapshot = await GetLastSnapshotAsync();
            lock (snapshotCacheLock)
            {
                lastSnapshotCached = lastSnapshot;
            }
        }

        var now = DateTime.UtcNow;

        var lastEventNumber = lastSnapshot?.EventNumber ?? 0;
        var lastTime = lastSnapshot?.Timestamp ?? DateTime.MinValue;

        var eventCountPassed =
            currentEventNumber - lastEventNumber >= snapshotOptions.Trigger.EventThreshold;
        var timePassed = HasTimePassed(lastTime, snapshotOptions.Trigger.Frequency);

        var shouldTake = snapshotOptions.Trigger.Mode switch
        {
            SnapshotTriggerMode.EventCount => eventCountPassed,
            SnapshotTriggerMode.Time => timePassed,
            SnapshotTriggerMode.Either => eventCountPassed || timePassed,
            SnapshotTriggerMode.Both => eventCountPassed && timePassed,
            _ => false
        };

        if (!shouldTake)
            return;

        var id = await TakeSnapshotAsync(currentEventNumber);

        var updatedSnapshot = new SnapshotMetadata
        {
            SnapshotId = id,
            Timestamp = now,
            EventNumber = currentEventNumber
        };

        lock (snapshotCacheLock)
        {
            lastSnapshotCached = updatedSnapshot;
        }

        await PruneOldSnapshotsAsync();
    }

    public async Task<string> TakeSnapshotAsync(long eventNumber)
    {
        ThrowIfSnapshotDisabled();
        await SnapshotLock.WaitAsync();
        try
        {
            if (replayContext is { IsReplaying: true, IsLoading: false })
                throw new InvalidOperationException(
                    "Cannot take a snapshot while in replay mode. Please stop the replay first."
                );

            var dateTime = DateTime.UtcNow;
            var snapshotId = $"snapshot_{dateTime:yyyyMMddHHmmss}_{eventNumber}";

            var registered = entityCollectionNameProvider.GetAllRegistered();

            var copiedAny = false;

            foreach (var (type, collectionName) in registered)
            {
                var method = typeof(SnapshotService).GetMethod(
                    nameof(CopyCollectionAsync),
                    BindingFlags.NonPublic | BindingFlags.Instance
                );
                var genericMethod = method!.MakeGenericMethod(type);
                var result = await (Task<bool>)
                    genericMethod.Invoke(this, new object[] { collectionName, snapshotId })!;
                copiedAny |= result;
            }

            if (!copiedAny)
                return snapshotId;

            var snapshotMeta = new BsonDocument
            {
                { "SnapshotId", snapshotId },
                { "EventNumber", eventNumber },
                { "Timestamp", dateTime }
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
        ThrowIfSnapshotDisabled();
        if (!replayContext.IsReplaying)
        {
            replayContext.StartReplay(ReplayMode.Debug);
            await mongoDbService.UseDebugEntityDatabase();
        }

        await SnapshotLock.WaitAsync();
        try
        {
            var registered = entityCollectionNameProvider.GetAllRegistered();
            var renamedBackups = new List<string>();

            try
            {
                if (replayContext.ReplayMode != ReplayMode.Debug)
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

                        if (!collections.Contains(originalName))
                            continue;

                        await mongoDbService
                            .GetEntityDatabase(true)
                            .RenameCollectionAsync(originalName, backupName);
                        renamedBackups.Add(originalName);
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

                if (replayContext.ReplayMode != ReplayMode.Debug)
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
            catch (Exception ex)
            {
                if (replayContext.ReplayMode != ReplayMode.Debug)
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
        ThrowIfSnapshotDisabled();
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
        ThrowIfSnapshotDisabled();
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
        ThrowIfSnapshotDisabled();
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
        ThrowIfSnapshotDisabled();
        var documents = await mongoDbService
            .GetEntityDatabase(true)
            .GetCollection<BsonDocument>(SnapshotMetadataCollection)
            .Find(_ => true)
            .Sort(Builders<BsonDocument>.Sort.Descending("Timestamp"))
            .ToListAsync();

        var snapshots = documents
            .Select(doc => BsonSerializer.Deserialize<SnapshotMetadata>(doc))
            .ToList();

        return snapshots;
    }

    public async Task<SnapshotMetadata?> GetLastSnapshotAsync()
    {
        ThrowIfSnapshotDisabled();
        return await mongoDbService
            .GetEntityDatabase(true)
            .GetCollection<SnapshotMetadata>(SnapshotMetadataCollection)
            .Find(FilterDefinition<SnapshotMetadata>.Empty)
            .Sort(Builders<SnapshotMetadata>.Sort.Descending("Timestamp"))
            .FirstOrDefaultAsync();
    }

    public async Task DeleteSnapshotAsync(string snapshotId)
    {
        ThrowIfSnapshotDisabled();
        var database = mongoDbService.GetEntityDatabase(true);
        var collections = await (await database.ListCollectionNamesAsync()).ToListAsync();

        foreach (
            var collectionName in collections.Where(collectionName =>
                collectionName.EndsWith(snapshotId)
            )
        )
            await database.DropCollectionAsync(collectionName);

        var metadataCollection = database.GetCollection<SnapshotMetadata>(SnapshotMetadataCollection);
        var filter = Builders<SnapshotMetadata>.Filter.Eq(s => s.SnapshotId, snapshotId);
        await metadataCollection.DeleteOneAsync(filter);
    }

    private void ThrowIfSnapshotDisabled()
    {
        if (!snapshotOptions.Enabled)
            throw new InvalidOperationException(
                "Snapshots are disabled. Please enable them in the configuration."
            );
    }

    private async Task<bool> CopyCollectionAsync<T>(
        string originalCollectionName,
        string snapshotId
    )
    {
        var source = mongoDbService.GetCollection<T>(originalCollectionName, true);
        var snapshotName = $"{originalCollectionName}_{snapshotId}";
        var target = mongoDbService.GetCollection<T>(snapshotName, true);

        var allDocs = await source.Find(_ => true).ToListAsync();
        if (allDocs.Count == 0)
            return false;

        await target.InsertManyAsync(allDocs);
        return true;
    }

    private async Task RestoreCollectionAsync<T>(string originalCollectionName, string snapshotId)
    {
        var snapshotName = $"{originalCollectionName}_{snapshotId}";
        var source = mongoDbService.GetCollection<T>(snapshotName, true);
        var target = mongoDbService.GetCollection<T>(
            originalCollectionName,
            replayContext.ReplayMode != ReplayMode.Debug
        );

        var docs = await source.Find(_ => true).ToListAsync();
        await mongoDbService
            .GetEntityDatabase(replayContext.ReplayMode != ReplayMode.Debug)
            .DropCollectionAsync(originalCollectionName);

        if (docs.Count == 0)
            return;
        await target.InsertManyAsync(docs);
    }

    private static bool HasTimePassed(DateTime lastSnapshot, SnapshotFrequency frequency)
    {
        var now = DateTime.UtcNow;
        return frequency switch
        {
            SnapshotFrequency.Day => (now - lastSnapshot).TotalDays >= 1,
            SnapshotFrequency.Week => (now - lastSnapshot).TotalDays >= 7,
            SnapshotFrequency.Month => (now - lastSnapshot).TotalDays >= 30,
            SnapshotFrequency.Year => (now - lastSnapshot).TotalDays >= 365,
            _ => false
        };
    }

    private async Task PruneOldSnapshotsAsync()
    {
        if (
            !snapshotOptions.Enabled
            || snapshotOptions.Retention.Strategy == SnapshotRetentionStrategy.All
        )
            return;

        var snapshots = await GetAllSnapshotsAsync();

        switch (snapshotOptions.Retention.Strategy)
        {
            case SnapshotRetentionStrategy.Count:
            {
                var toDelete = snapshots
                    .OrderByDescending(s => s.Timestamp)
                    .Skip(snapshotOptions.Retention.MaxCount)
                    .ToList();

                foreach (var snapshot in toDelete)
                    await DeleteSnapshotAsync(snapshot.SnapshotId);
                break;
            }
            case SnapshotRetentionStrategy.Time:
            {
                var threshold = DateTime.UtcNow.AddDays(-snapshotOptions.Retention.MaxAgeDays);

                var toDelete = snapshots.Where(s => s.Timestamp < threshold).ToList();

                foreach (var snapshot in toDelete)
                    await DeleteSnapshotAsync(snapshot.SnapshotId);
                break;
            }
        }
    }
}