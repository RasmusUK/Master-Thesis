using SpotQuoteBooking.EventSource.Core;

namespace SpotQuoteBooking.Shared;

public record ShippingDetails(
    ICollection<Colli> Collis,
    string Description,
    DateTime ReadyToLoadDate
);
