namespace SpotQuoteBooking.Shared;

public record Colli(
    int NumberOfUnits,
    ColliType ColliType,
    double Length,
    double Width,
    double Height,
    double Weight,
    double Cbm
);
