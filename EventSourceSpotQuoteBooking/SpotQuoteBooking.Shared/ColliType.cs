namespace SpotQuoteBooking.Shared;

public record ColliType(string Value) : IComparable
{
    public static readonly ColliType Pallet = new("Pallet");
    public static readonly ColliType Box = new("Box");
    public static readonly ColliType Container = new("Container");

    public override string ToString() => Value;

    public int CompareTo(object? obj)
    {
        if (obj is ColliType other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(ColliType)}");
    }

    public static IReadOnlyCollection<ColliType> GetAll() =>
        new List<ColliType> { Pallet, Box, Container }
            .OrderBy(x => x.Value)
            .ToList();
}
