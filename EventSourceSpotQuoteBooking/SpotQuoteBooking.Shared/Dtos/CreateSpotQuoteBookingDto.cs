namespace SpotQuoteBooking.Shared.Dtos;

public class CreateSpotQuoteBookingDto
{
    public AddressDto AddressFrom { get; set; }
    public AddressDto AddressTo { get; set; }
    public string Direction { get; set; }
    public string TransportMode { get; set; }
    public string Incoterm { get; set; }
    public ShippingDetailsDto ShippingDetails { get; set; }
    public DateTime ValidUntil { get; set; }
}
