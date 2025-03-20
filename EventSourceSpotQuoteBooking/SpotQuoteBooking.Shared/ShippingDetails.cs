namespace SpotQuoteBooking.Shared;

public record ShippingDetails(
    ICollection<Colli> Collis,
    string Description,
    DateTime ReadyToLoadDate
);
