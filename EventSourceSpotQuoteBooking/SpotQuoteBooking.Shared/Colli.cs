using SpotQuoteBooking.Shared;

namespace SpotQuoteBooking.EventSource.Core;

public record Colli(
    int NumberOfUnits,
    ColliType ColliType,
    double Length,
    double Width,
    double Height,
    double Weight,
    double Cbm
);
