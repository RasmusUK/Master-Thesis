namespace EventSourcingFramework.Infrastructure.EntityStore.Exceptions;

public class EntityStoreException : Exception
{
    public EntityStoreException(string message)
        : base(message) { }
}
