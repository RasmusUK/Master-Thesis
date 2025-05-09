namespace SpotQuoteApp.Core.ValueObjects.Enums;

public record BookingProperty(string Value) : IComparable
{
    public static readonly BookingProperty NeutralDelivery = new("Neutral Delivery");
    public static readonly BookingProperty NonStackable = new("Non-Stackable");
    public static readonly BookingProperty ExportDeclaration = new("Export Declaration");

    public override string ToString() => Value;

    public int CompareTo(object? obj)
    {
        if (obj is BookingProperty other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(BookingProperty)}");
    }

    public static IReadOnlyCollection<BookingProperty> GetAll() =>
        new List<BookingProperty> { NeutralDelivery, NonStackable, ExportDeclaration }
            .OrderBy(x => x.Value)
            .ToList();
}
