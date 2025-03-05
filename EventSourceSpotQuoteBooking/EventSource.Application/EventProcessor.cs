using EventSource.Core;
using EventSource.Core.Interfaces;

namespace EventSource.Application;

public class EventProcessor : IEventProcessor
{
    private readonly IEventStore eventStore;
    private readonly IEntityStore entityStore;
    private readonly Dictionary<Type, Type> handlers = new();

    public EventProcessor(IEventStore eventStore, IEntityStore entityStore)
    {
        this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        this.entityStore = entityStore;
    }

    public void RegisterHandler<TEvent, TEntity>()
        where TEvent : Event
        where TEntity : Entity => handlers.Add(typeof(TEvent), typeof(TEntity));

    public async Task<Entity> ProcessAsync(Event e)
    {
        await eventStore.SaveEventAsync(e);
        var entity = await ProcessEntityAsync(e);
        await entityStore.SaveEntityAsync(entity);
        return entity;
    }

    public async Task<Entity> ProcessHistoryAsync(Event e) => await ProcessEntityAsync(e);

    private async Task<Entity> ProcessEntityAsync(Event e)
    {
        var entity = await GetEntityAsync(e);
        ((dynamic)entity).Apply(e);
        return (Entity)entity;
    }

    public async Task<TEntity> GetEntityAsync<TEntity>(Guid id)
        where TEntity : Entity =>
        await entityStore.GetEntityAsync<TEntity>(id) ?? Entity.Create<TEntity>(id);

    private async Task<object> GetEntityAsync(Event e)
    {
        var type = handlers[e.GetType()];
        var x = typeof(EventProcessor).GetMethod(nameof(GetEntityAsync));
        if (x is null)
            throw new InvalidOperationException(
                $"Could not find a method named {nameof(GetEntityAsync)}"
            );
        var y = x.MakeGenericMethod(type);
        var task = y.Invoke(this, new object[] { e.EntityId });
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
