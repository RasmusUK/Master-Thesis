namespace SpotQuoteBooking.Shared;

public record TransportMode(string Value) : IComparable
{
    public static readonly TransportMode Air = new("Air");
    public static readonly TransportMode Sea = new("Sea");
    public static readonly TransportMode Road = new("Road");
    public static readonly TransportMode Rail = new("Rail");
    public static readonly TransportMode Courier = new("Courier");

    public override string ToString() => Value;

    public int CompareTo(object? obj)
    {
        if (obj is TransportMode other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(TransportMode)}");
    }
}
