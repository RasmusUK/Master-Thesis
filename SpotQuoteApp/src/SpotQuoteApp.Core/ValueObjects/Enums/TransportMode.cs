using System.Reflection;

namespace SpotQuoteApp.Core.ValueObjects.Enums;

public record TransportMode(string Value) : IComparable
{
    public static readonly TransportMode Air = new("Air");
    public static readonly TransportMode Sea = new("Sea");
    public static readonly TransportMode Road = new("Road");
    public static readonly TransportMode Courier = new("Courier");

    public int CompareTo(object? obj)
    {
        if (obj is TransportMode other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(TransportMode)}");
    }

    public override string ToString()
    {
        return Value;
    }

    public static TransportMode FromString(string value)
    {
        return GetAll()
                .FirstOrDefault(c => c.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Invalid {nameof(TransportMode)}: {value}");
    }

    public static IReadOnlyCollection<TransportMode> GetAll()
    {
        return typeof(TransportMode)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => f.GetValue(null))
            .OfType<TransportMode>()
            .ToList();
    }
}
