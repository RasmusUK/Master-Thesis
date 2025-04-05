using EventSource.Core;
using SpotQuoteBooking.Shared;

namespace SpotQuoteBooking.EventSource.Core;

public class SpotQuoteBooking : Entity
{
    public Address AddressFrom { get; set; }
    public Address AddressTo { get; set; }
    public Direction Direction { get; set; }
    public TransportMode TransportMode { get; set; }
    public Incoterm Incoterm { get; set; }
    public BookingStatus Status { get; set; }
    public ShippingDetails ShippingDetails { get; set; }
    public DateTime ValidUntil { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Customer Customer { get; set; }
}
