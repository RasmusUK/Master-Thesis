using EventSourcingFramework.Core;
using EventSourcingFramework.Persistence.Interfaces;

namespace EventSourcingFramework.Persistence;

public class EntityMigrator : IEntityMigrator
{
    private readonly Dictionary<(Type, int), Func<IEntity, IEntity>> migrations = new();

    public void Register<TFrom, TTo>(int fromVersion, Func<TFrom, TTo> migration)
        where TFrom : IEntity
        where TTo : IEntity
    {
        migrations[(typeof(TFrom), fromVersion)] = e => migration((TFrom)e);
    }

    public IEntity Migrate(IEntity oldInstance, Type fromType, Type toType, int fromVersion)
    {
        if (fromType == toType)
            return oldInstance;

        if (!migrations.TryGetValue((fromType, fromVersion), out var migrator))
            throw new Exception($"No migration registered from {fromType.Name} v{fromVersion}");

        return migrator(oldInstance);
    }
}
