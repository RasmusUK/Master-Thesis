namespace SpotQuoteBooking.Shared;

public record ColliType(string Value) : IComparable
{
    public static readonly ColliType Pallet = new("Pallet");
    public static readonly ColliType Box = new("Box");
    public static readonly ColliType Container = new("Container");

    public static ColliType GetColliType(string value)
    {
        return value switch
        {
            "Pallet" => Pallet,
            "Box" => Box,
            "Container" => Container,
            _ => throw new ArgumentException($"Unknown colli type: {value}"),
        };
    }

    public int CompareTo(object? obj)
    {
        if (obj is ColliType other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(ColliType)}");
    }
}
