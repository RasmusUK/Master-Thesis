using System.Reflection;

namespace SpotQuoteApp.Core.ValueObjects.Enums;

public record BookingStatus(string Value) : IComparable
{
    public static readonly BookingStatus SpotQuote = new("Spot Quote");
    public static readonly BookingStatus Accepted = new("Accepted");
    public static readonly BookingStatus Draft = new("Draft");
    public static readonly BookingStatus Requote = new("Requote");
    public static readonly BookingStatus PendingSubmit = new("Pending Submit");
    public static readonly BookingStatus NotAccepted = new("Not Accepted");

    public int CompareTo(object? obj)
    {
        if (obj is BookingStatus other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(BookingStatus)}");
    }

    public override string ToString()
    {
        return Value;
    }

    public static IReadOnlyCollection<BookingStatus> GetAll()
    {
        return typeof(BookingStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => f.GetValue(null))
            .OfType<BookingStatus>()
            .ToList();
    }
}
