namespace SpotQuoteBooking.Shared;

public record ColliType(string Value)
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
}
