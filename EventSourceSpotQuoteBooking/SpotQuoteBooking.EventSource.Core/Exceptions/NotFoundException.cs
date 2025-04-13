namespace SpotQuoteBooking.EventSource.Core.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message) { }
}
