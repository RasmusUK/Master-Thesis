using EventSource.Core;
using EventSource.Core.Interfaces;

namespace EventSource.Application;

public class EventProcessor : IEventProcessor
{
    private readonly IEventStore eventStore;
    private readonly IEntityStore entityStore;
    private readonly Dictionary<Type, Type> eventTypeToEntityType = new();
    private readonly Dictionary<Type, List<object>> eventTypeToHandler = new();

    public EventProcessor(IEventStore eventStore, IEntityStore entityStore)
    {
        this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        this.entityStore = entityStore;
    }

    public void RegisterEventToEntity<TEvent, TEntity>()
        where TEvent : Event
        where TEntity : Entity => eventTypeToEntityType.TryAdd(typeof(TEvent), typeof(TEntity));

    public void RegisterEventHandler<TEvent>(IEventHandler<TEvent> eventHandler)
        where TEvent : Event
    {
        var eventType = typeof(TEvent);
        if (!eventTypeToHandler.ContainsKey(eventType))
            eventTypeToHandler.Add(eventType, new List<object>());

        eventTypeToHandler[eventType].Add(eventHandler);
    }

    public async Task<Entity> ProcessAsync(Event e)
    {
        await eventStore.SaveEventAsync(e);
        var entity = await ProcessEntityEventAsync(e);
        await entityStore.SaveEntityAsync(entity);
        await ProcessEventHandlerAsync(e);
        return entity;
    }

    public async Task<Entity> ProcessReplayAsync(Event e)
    {
        var entity = await ProcessEntityEventAsync(e);
        await entityStore.SaveEntityAsync(entity);
        await ProcessEventHandlerAsync(e);
        return entity;
    }

    public async Task<Entity> ProcessHistoryAsync(Event e)
    {
        var entity = await ProcessEntityEventAsync(e);
        await ProcessEventHandlerAsync(e);
        return entity;
    }

    private async Task<Entity> ProcessEntityEventAsync(Event e)
    {
        var entity = await GetEntityAsync(e);
        ((dynamic)entity).Apply(e);
        return (Entity)entity;
    }

    private async Task ProcessEventHandlerAsync(Event e)
    {
        var eventType = e.GetType();
        if (eventTypeToHandler.TryGetValue(eventType, out var handlers))
        {
            foreach (var handler in handlers)
            {
                var handleMethod = handler.GetType().GetMethod(nameof(EventHandler.HandleAsync));
                if (handleMethod is null)
                    throw new InvalidOperationException(
                        $"Could not find a method named {nameof(EventHandler.HandleAsync)}"
                    );
                var task = handleMethod.Invoke(handler, new object[] { e });
                if (task is null)
                    throw new InvalidOperationException(
                        $"Could not invoke {nameof(GetEntityAsync)}"
                    );
                var entityTask = (Task)task;
                await entityTask;
            }
        }
    }

    public async Task<TEntity> GetEntityAsync<TEntity>(Guid id)
        where TEntity : Entity =>
        await entityStore.GetEntityAsync<TEntity>(id) ?? Entity.Create<TEntity>(id);

    private async Task<object> GetEntityAsync(Event e)
    {
        var hasEntityType = eventTypeToEntityType.TryGetValue(e.GetType(), out var type);
        if (!hasEntityType || type is null)
            throw new InvalidOperationException(
                $"Could not find entity type for event {e.GetType()}"
            );

        var methodInfo = typeof(EventProcessor).GetMethod(nameof(GetEntityAsync));
        if (methodInfo is null)
            throw new InvalidOperationException(
                $"Could not find a method named {nameof(GetEntityAsync)}"
            );
        var genericMethod = methodInfo.MakeGenericMethod(type);
        var task = genericMethod.Invoke(this, new object[] { e.EntityId });
        if (task is null)
            throw new InvalidOperationException($"Could not invoke {nameof(GetEntityAsync)}");

        var entityTask = (Task)task;
        await entityTask;

        var resultProperty = entityTask.GetType().GetProperty("Result");
        if (resultProperty is null)
            throw new InvalidOperationException("Could not retrieve result from the task");

        var entity = resultProperty.GetValue(entityTask);
        if (entity is null)
            throw new InvalidOperationException("Retrieved entity is null");
        return entity;
    }
}
