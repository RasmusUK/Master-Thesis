using System.Reflection;

namespace SpotQuoteApp.Core.ValueObjects.Enums;

public record CostType(string Value) : IComparable
{
    public static readonly CostType PerShipment = new("Per Shipment");
    public static readonly CostType PerKg = new("Per Kg");
    public static readonly CostType PerCbm = new("Per Cbm");

    public int CompareTo(object? obj)
    {
        if (obj is CostType other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(CostType)}");
    }

    public override string ToString()
    {
        return Value;
    }

    public static CostType FromString(string value)
    {
        return GetAll()
                .FirstOrDefault(c => c.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Invalid {nameof(CostType)}: {value}");
    }

    public static IReadOnlyCollection<CostType> GetAll()
    {
        return typeof(CostType)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => f.GetValue(null))
            .OfType<CostType>()
            .ToList();
    }
}
