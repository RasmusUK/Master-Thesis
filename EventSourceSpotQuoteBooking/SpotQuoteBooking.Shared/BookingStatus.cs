namespace SpotQuoteBooking.EventSource.Core;

public record BookingStatus(string Value)
{
    public static readonly BookingStatus SpotQuote = new("Spot Quote");
    public static readonly BookingStatus Accepted = new("Accepted");
    public static readonly BookingStatus Draft = new("Draft");
    public static readonly BookingStatus Requote = new("Requote");
    public static readonly BookingStatus PendingSubmit = new("Pending Submit");
    public static readonly BookingStatus NotAccepted = new("Not Accepted");
}
