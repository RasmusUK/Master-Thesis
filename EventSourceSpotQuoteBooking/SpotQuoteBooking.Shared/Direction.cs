namespace SpotQuoteBooking.EventSource.Core;

public record Direction(string Value)
{
    public static readonly Direction Import = new("Import");
    public static readonly Direction Export = new("Export");

    public static Direction GetDirection(string value)
    {
        return value switch
        {
            "Import" => Import,
            "Export" => Export,
            _ => throw new ArgumentException($"Unknown direction: {value}"),
        };
    }
}
