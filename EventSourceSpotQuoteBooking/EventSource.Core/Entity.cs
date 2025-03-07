using System.Reflection;

namespace EventSource.Core;

public abstract class Entity
{
    public Guid Id { get; init; }

    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    public abstract void Apply(Event e);

    public static TEntity Create<TEntity>()
        where TEntity : Entity
    {
        var entity = Activator.CreateInstance(typeof(TEntity));

        if (entity is null)
            throw new InvalidOperationException(
                $"Could not create an instance of {typeof(TEntity).Name}"
            );
        return (TEntity)entity;
    }

    private void Apply(object e) { }
}
