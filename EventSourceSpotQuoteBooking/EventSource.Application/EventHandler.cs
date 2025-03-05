using System.Reflection;
using EventSource.Core;
using EventSource.Core.Interfaces;

namespace EventSource.Application;

public class EventHandler : IEventHandler
{
    private readonly IEntityStore entityStore;

    public EventHandler(IEntityStore entityStore)
    {
        this.entityStore = entityStore;
    }

    public async Task HandleAsync<TAggregateRoot>(Event e)
        where TAggregateRoot : Entity
    {
        var aggregateRoot =
            await entityStore.GetEntityAsync<TAggregateRoot>(e.AggregateId)
            ?? CreateAggregateRoot<TAggregateRoot>(e.AggregateId);

        aggregateRoot.Apply(e);
        await AfterHandleAsync(aggregateRoot);
    }

    private Task AfterHandleAsync<TAggregateRoot>(TAggregateRoot aggregateRoot)
        where TAggregateRoot : Entity => entityStore.SaveEntityAsync(aggregateRoot);

    private TAggregateRoot CreateAggregateRoot<TAggregateRoot>(Guid id)
    {
        var aggregateRootObject = Activator.CreateInstance(typeof(TAggregateRoot));

        if (aggregateRootObject is null)
            throw new InvalidOperationException(
                $"Could not create an instance of {typeof(TAggregateRoot).Name}"
            );

        var idField = typeof(Entity).GetField(
            $"<{nameof(Entity.Id)}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        if (idField is null)
            throw new InvalidOperationException(
                $"Could not find a field named {nameof(Entity.Id)}"
            );

        idField.SetValue(aggregateRootObject, id);

        return (TAggregateRoot)aggregateRootObject;
    }
}
