using EventSource.Core;
using EventSource.Core.Interfaces;

namespace EventSource.Persistence.Stores;

public class MongoDbEntityHistoryStore : IEntityHistoryStore
{
    private readonly IEventStore eventStore;
    private readonly IEntityStore entityStore;
    private readonly IEventProcessor eventProcessor;

    public MongoDbEntityHistoryStore(
        IEventStore eventStore,
        IEntityStore entityStore,
        IEventProcessor eventProcessor
    )
    {
        this.eventStore = eventStore;
        this.entityStore = entityStore;
        this.eventProcessor = eventProcessor;
    }

    public async Task<IReadOnlyCollection<T>> GetEntityHistoryAsync<T>(Guid id)
        where T : Entity
    {
        var events = await eventStore.GetEventsAsync(id);
        var entities = new List<T>();

        foreach (var e in events)
        {
            await eventProcessor.ProcessReplayAsync(e);
            var entity = await entityStore.GetEntityAsync<T>(id);
            if (entity is not null)
                entities.Add(entity);
        }

        return entities;
    }
}
