namespace SpotQuoteApp.Core.ValueObjects.Enums;

public record TransportMode(string Value) : IComparable
{
    public static readonly TransportMode Air = new("Air");
    public static readonly TransportMode Sea = new("Sea");
    public static readonly TransportMode Road = new("Road");
    public static readonly TransportMode Courier = new("Courier");

    public override string ToString() => Value;
    
    public static TransportMode FromString(string value) =>
        GetAll().FirstOrDefault(c => c.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException($"Invalid {nameof(TransportMode)}: {value}");

    public int CompareTo(object? obj)
    {
        if (obj is TransportMode other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(TransportMode)}");
    }

    public static IReadOnlyCollection<TransportMode> GetAll() =>
        typeof(TransportMode)
            .GetFields(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
            )
            .Select(f => f.GetValue(null))
            .OfType<TransportMode>()
            .ToList();
}
