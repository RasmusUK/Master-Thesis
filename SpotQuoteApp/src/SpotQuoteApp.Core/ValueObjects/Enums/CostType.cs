namespace SpotQuoteApp.Core.ValueObjects.Enums;

public record CostType(string Value) : IComparable
{
    public static readonly CostType PerShipment = new("Per Shipment");
    public static readonly CostType PerKg = new("Per Kg");
    public static readonly CostType PerCbm = new("Per Cbm");

    public override string ToString() => Value;
    
    public static CostType FromString(string value) =>
        GetAll().FirstOrDefault(c => c.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException($"Invalid {nameof(CostType)}: {value}");

    public int CompareTo(object? obj)
    {
        if (obj is CostType other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(CostType)}");
    }

    public static IReadOnlyCollection<CostType> GetAll() =>
        typeof(CostType)
            .GetFields(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
            )
            .Select(f => f.GetValue(null))
            .OfType<CostType>()
            .ToList();
}
