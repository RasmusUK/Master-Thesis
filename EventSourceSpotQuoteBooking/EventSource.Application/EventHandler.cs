using System.Reflection;
using EventSource.Core;
using EventSource.Core.Interfaces;

namespace EventSource.Application;

public class EventHandler : IEventHandler
{
    private readonly IAggregateRootStore aggregateRootStore;

    public EventHandler(IAggregateRootStore aggregateRootStore)
    {
        this.aggregateRootStore = aggregateRootStore;
    }

    public async Task HandleAsync<TAggregateRoot>(Event e)
        where TAggregateRoot : AggregateRoot
    {
        var aggregateRoot =
            await aggregateRootStore.GetAggregateRootAsync<TAggregateRoot>(e.AggregateId)
            ?? CreateAggregateRoot<TAggregateRoot>(e.AggregateId);

        aggregateRoot.Apply(e);
        await AfterHandleAsync(aggregateRoot);
    }

    private Task AfterHandleAsync<TAggregateRoot>(TAggregateRoot aggregateRoot)
        where TAggregateRoot : AggregateRoot =>
        aggregateRootStore.SaveAggregateRootAsync(aggregateRoot);

    private TAggregateRoot CreateAggregateRoot<TAggregateRoot>(Guid id)
    {
        var aggregateRootObject = Activator.CreateInstance(typeof(TAggregateRoot));

        if (aggregateRootObject is null)
            throw new InvalidOperationException(
                $"Could not create an instance of {typeof(TAggregateRoot).Name}"
            );

        var idField = typeof(AggregateRoot).GetField(
            $"<{nameof(AggregateRoot.Id)}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        if (idField is null)
            throw new InvalidOperationException(
                $"Could not find a field named {nameof(AggregateRoot.Id)}"
            );

        idField.SetValue(aggregateRootObject, id);

        return (TAggregateRoot)aggregateRootObject;
    }
}
