namespace SpotQuoteApp.Core.ValueObjects.Enums;

public record LocationType(string Value)
{
    public static readonly LocationType ZipCode = new("ZipCode");
    public static readonly LocationType Airport = new("Airport");
    public static readonly LocationType Port = new("Port");

    public override string ToString() => Value;
}
