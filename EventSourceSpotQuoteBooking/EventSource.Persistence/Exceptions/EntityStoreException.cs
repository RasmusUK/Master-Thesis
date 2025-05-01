namespace EventSource.Persistence.Exceptions;

public class EntityStoreException : Exception
{
    public EntityStoreException(string message)
        : base(message) { }
}
