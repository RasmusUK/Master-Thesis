namespace EventSourcingFramework.Infrastructure.Repositories.Exceptions;

public class RepositoryException : Exception
{
    public RepositoryException(string message)
        : base(message)
    {
    }
}