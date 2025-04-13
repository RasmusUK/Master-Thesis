namespace SpotQuoteBooking.EventSource.Core.ValueObjects.Enums;

public record Direction(string Value) : IComparable
{
    public static readonly Direction Import = new("Import");
    public static readonly Direction Export = new("Export");

    public override string ToString() => Value;

    public int CompareTo(object? obj)
    {
        if (obj is Direction other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(Direction)}");
    }
}
