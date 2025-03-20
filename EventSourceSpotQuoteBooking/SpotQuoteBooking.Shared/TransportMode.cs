namespace SpotQuoteBooking.Shared;

public record TransportMode(string Value)
{
    public static readonly TransportMode Air = new("Air");
    public static readonly TransportMode Sea = new("Sea");
    public static readonly TransportMode Road = new("Road");
    public static readonly TransportMode Rail = new("Rail");
    public static readonly TransportMode Courier = new("Courier");

    public static TransportMode GetTransportMode(string value)
    {
        return value switch
        {
            "Air" => Air,
            "Sea" => Sea,
            "Road" => Road,
            "Rail" => Rail,
            "Courier" => Courier,
            _ => throw new ArgumentException($"Unknown transport mode: {value}"),
        };
    }
}
