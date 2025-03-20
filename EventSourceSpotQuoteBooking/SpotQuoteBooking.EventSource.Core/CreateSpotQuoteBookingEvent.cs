using EventSource.Core;
using SpotQuoteBooking.Shared;

namespace SpotQuoteBooking.EventSource.Core;

public record CreateSpotQuoteBookingEvent(
    Address AddressFrom,
    Address AddressTo,
    Direction Direction,
    TransportMode TransportMode,
    Incoterm Incoterm,
    ShippingDetails ShippingDetails
) : Event { }
