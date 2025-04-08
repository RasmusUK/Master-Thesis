namespace SpotQuoteBooking.Shared;

public record CostType(string Value) : IComparable
{
    public static readonly CostType PerShipment = new("Per Shipment");
    public static readonly CostType PerKg = new("Per Kg");
    public static readonly CostType PerCbm = new("Per Cbm");

    public override string ToString() => Value;

    public int CompareTo(object? obj)
    {
        if (obj is CostType other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(CostType)}");
    }

    public static IReadOnlyCollection<CostType> GetAll() =>
        new List<CostType> { PerShipment, PerKg, PerCbm }
            .OrderBy(x => x.Value)
            .ToList();
}
