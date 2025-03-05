using System.Reflection;

namespace EventSource.Core;

public abstract class Entity
{
    public Guid Id { get; init; } = Guid.NewGuid();

    protected Entity() { }

    public abstract void Apply(Event e);

    public static TEntity Create<TEntity>(Guid id)
        where TEntity : Entity
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty", nameof(id));

        var entity = Activator.CreateInstance(typeof(TEntity));

        if (entity is null)
            throw new InvalidOperationException(
                $"Could not create an instance of {typeof(TEntity).Name}"
            );

        var idField = typeof(Entity).GetField(
            $"<{nameof(Id)}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        if (idField is null)
            throw new InvalidOperationException($"Could not find a field named {nameof(Id)}");

        idField.SetValue(entity, id);

        return (TEntity)entity;
    }

    private void Apply(object e) { }
}
