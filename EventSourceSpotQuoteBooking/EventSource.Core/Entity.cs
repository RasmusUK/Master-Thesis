using System.Reflection;

namespace EventSource.Core;

public abstract class Entity
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public void Apply(Event e)
    {
        var method = GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .FirstOrDefault(m =>
                m.Name == nameof(Apply)
                && m.GetParameters().Length == 1
                && m.GetParameters()[0].ParameterType == e.GetType()
            );

        if (method is not null)
            method.Invoke(this, new object[] { e });
        else
            Apply(e);
    }

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
