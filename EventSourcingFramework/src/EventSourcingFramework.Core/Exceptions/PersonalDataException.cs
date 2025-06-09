namespace EventSourcingFramework.Core.Exceptions;

public class PersonalDataException : Exception
{
    public PersonalDataException(string message)
        : base(message)
    {
    }
}