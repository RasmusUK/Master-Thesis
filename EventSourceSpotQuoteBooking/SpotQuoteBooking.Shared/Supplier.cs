namespace SpotQuoteBooking.Shared;

public record Supplier(string Value) : IComparable
{
    public static readonly Supplier DHL = new("DHL");
    public static readonly Supplier FedEx = new("FedEx");
    public static readonly Supplier UPS = new("UPS");
    public static readonly Supplier Aramex = new("Aramex");
    public static readonly Supplier Maersk = new("Maersk");
    public static readonly Supplier DB_Schenker = new("DB Schenker");

    public override string ToString() => Value;

    public int CompareTo(object? obj)
    {
        if (obj is ChargeType other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(ChargeType)}");
    }
}
