using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Core.ValueObjects;

public record Colli(
    int NumberOfUnits,
    ColliType Type,
    double Length,
    double Width,
    double Height,
    double Weight
)
{
    public double Cbm => Length * Width * Height / 1000000;
}
