namespace SpotQuoteBooking.EventSource.Core;

public record ShippingDetails(
    ICollection<Colli> Collis,
    string Description,
    DateTime ReadyToLoadDate
);
