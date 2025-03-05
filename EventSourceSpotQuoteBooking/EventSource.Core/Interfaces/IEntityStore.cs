namespace EventSource.Core.Interfaces;

public interface IEntityStore
{
    Task SaveEntityAsync(Entity entity);
    Task<T?> GetEntityAsync<T>(Guid id)
        where T : Entity;
    Task<IReadOnlyCollection<Entity>> GetEntitiesAsync();
}
