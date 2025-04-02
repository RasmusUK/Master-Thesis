using EventSource.Core;

namespace SpotQuoteBooking.EventSource.Core;

public class SpotQuoteBooking : Entity
{
    public Address AddressFrom { get; set; }
    public Address AddressTo { get; set; }
    public Direction Direction { get; set; }
    public TransportMode TransportMode { get; set; }
    public Incoterm Incoterm { get; set; }
    public ShippingDetails ShippingDetails { get; set; }
    public DateTime ValidUntil { get; set; }
}
