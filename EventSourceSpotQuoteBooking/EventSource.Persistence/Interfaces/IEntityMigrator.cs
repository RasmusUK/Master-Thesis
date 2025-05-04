using EventSource.Core;
using MongoDB.Bson;

namespace EventSource.Persistence.Interfaces;

public interface IEntityMigrator
{
    void Register<TFrom, TTo>(int fromVersion, Func<TFrom, TTo> migration)
        where TFrom : IEntity
        where TTo : IEntity;

    IEntity Migrate(IEntity from, Type fromType, Type toType, int fromVersion);
}
