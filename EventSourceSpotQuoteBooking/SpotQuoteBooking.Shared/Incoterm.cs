namespace SpotQuoteBooking.Shared;

public record Incoterm(string Value) : IComparable
{
    public static readonly Incoterm EXW = new("EXW");
    public static readonly Incoterm FCA = new("FCA");
    public static readonly Incoterm FAS = new("FAS");
    public static readonly Incoterm FOB = new("FOB");
    public static readonly Incoterm CFR = new("CFR");
    public static readonly Incoterm CIF = new("CIF");
    public static readonly Incoterm CPT = new("CPT");
    public static readonly Incoterm CIP = new("CIP");
    public static readonly Incoterm DAP = new("DAP");
    public static readonly Incoterm DPU = new("DPU");
    public static readonly Incoterm DDP = new("DDP");
    public static readonly Incoterm DDU = new("DDU");
    public static readonly Incoterm DAT = new("DAT");

    public int CompareTo(object? obj)
    {
        if (obj is Incoterm other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(Incoterm)}");
    }
}
