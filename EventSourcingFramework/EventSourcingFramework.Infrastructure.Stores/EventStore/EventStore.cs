using EventSourcing.Framework.Infrastructure.Shared.Configuration.Options;
using EventSourcing.Framework.Infrastructure.Shared.Models.Events;
using EventSourcingFramework.Application.Abstractions;
using EventSourcingFramework.Application.Abstractions.Snapshots;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Core.Models.Events;
using EventSourcingFramework.Infrastructure.Abstractions.EventStore;
using EventSourcingFramework.Infrastructure.Abstractions.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EventSourcingFramework.Infrastructure.Stores.EventStore;

public class EventStore : IEventStore
{
    private readonly IMongoCollection<MongoEventBase> collection;
    private readonly IEventSequenceGenerator sequenceGenerator;
    private readonly IPersonalDataService personalDataService;
    private readonly ISnapshotService snapshotService;
    private readonly EventSourcingOptions eventSourcingOptions;

    public EventStore(
        IMongoDbService mongoDbService,
        IEventSequenceGenerator sequenceGenerator,
        IPersonalDataService personalDataService,
        ISnapshotService snapshotService,
        IOptions<EventSourcingOptions> eventSourcingOptions
    )
    {
        this.sequenceGenerator = sequenceGenerator;
        this.personalDataService = personalDataService;
        this.snapshotService = snapshotService;
        this.eventSourcingOptions = eventSourcingOptions.Value;
        collection = mongoDbService.EventCollection;
    }

    public async Task InsertEventAsync(IEvent e)
    {
        if (!eventSourcingOptions.EnableEventStore)
            return;

        if (e is not MongoEventBase eventBase)
            throw new ArgumentException("Event must be of type EventBase", nameof(e));

        if (await EventExistsAsync(eventBase))
            return;

        eventBase.EventNumber = await sequenceGenerator.GetNextSequenceNumberAsync();

        await personalDataService.StripAndStoreAsync(eventBase);

        await collection.InsertOneAsync(eventBase);

        await snapshotService.TakeSnapshotIfNeededAsync(eventBase.EventNumber);
    }

    public async Task<IEvent?> GetEventByIdAsync(Guid id)
    {
        var evt = await collection.Find(e => e.Id == id).FirstOrDefaultAsync();
        if (evt is not null)
            await personalDataService.RestoreAsync(evt);
        return evt;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsAsync()
    {
        var events = await collection.Find(_ => true).ToListAsync();
        await Task.WhenAll(Enumerable.Select(events, e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsUntilAsync(DateTime until)
    {
        var events = await collection.Find(e => e.Timestamp <= until).ToListAsync();
        await Task.WhenAll(Enumerable.Select(events, e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromAsync(DateTime from)
    {
        var events = await collection.Find(e => e.Timestamp >= from).ToListAsync();
        await Task.WhenAll(Enumerable.Select(events, e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromUntilAsync(
        DateTime from,
        DateTime until
    )
    {
        var events = await collection
            .Find(e => e.Timestamp >= from && e.Timestamp <= until)
            .ToListAsync();
        await Task.WhenAll(Enumerable.Select(events, e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsByEntityIdAsync(Guid entityId)
    {
        var events = await collection.Find(e => e.EntityId == entityId).ToListAsync();
        await Task.WhenAll(Enumerable.Select(events, e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsByEntityIdUntilAsync(
        Guid entityId,
        DateTime until
    )
    {
        var events = await collection
            .Find(e => e.EntityId == entityId && e.Timestamp <= until)
            .ToListAsync();
        await Task.WhenAll(Enumerable.Select(events, e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsByEntityIdFromUntilAsync(
        Guid entityId,
        DateTime from,
        DateTime until
    )
    {
        var events = await collection
            .Find(e => e.EntityId == entityId && e.Timestamp >= from && e.Timestamp <= until)
            .ToListAsync();
        await Task.WhenAll(Enumerable.Select(events, e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromAsync(long fromEventNumber)
    {
        var events = await collection
            .Find(e => e.EventNumber >= fromEventNumber)
            .SortBy(e => e.EventNumber)
            .ToListAsync();
        await Task.WhenAll(Enumerable.Select(events, e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsUntilAsync(long untilEventNumber)
    {
        var events = await collection
            .Find(e => e.EventNumber <= untilEventNumber)
            .SortBy(e => e.EventNumber)
            .ToListAsync();
        await Task.WhenAll(Enumerable.Select(events, e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromUntilAsync(
        long fromEventNumber,
        long untilEventNumber
    )
    {
        var events = await collection
            .Find(e => e.EventNumber >= fromEventNumber && e.EventNumber <= untilEventNumber)
            .SortBy(e => e.EventNumber)
            .ToListAsync();
        await Task.WhenAll(Enumerable.Select(events, e => personalDataService.RestoreAsync(e)));
        return events;
    }

    private Task<bool> EventExistsAsync(MongoEventBase e) =>
        collection.Find(x => x.Id == e.Id).AnyAsync();
}
