using EventSource.Core;
using SpotQuoteBooking.Shared;

namespace SpotQuoteBooking.EventSource.Core;

public class SpotQuoteBooking : Entity
{
    public Address? AddressFrom { get; set; }
    public Address? AddressTo { get; set; }
    public Direction? Direction { get; set; }
    public TransportMode? TransportMode { get; set; }
    public Incoterm? Incoterm { get; set; }
    public BookingStatus? Status { get; set; }
    public ShippingDetails? ShippingDetails { get; set; }
    public DateTime ValidUntil { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Customer? Customer { get; set; }
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public double TotalWeight
    {
        get => ShippingDetails?.Collis.Sum(c => c.Weight) ?? 0;
    }
    public double TotalCbm
    {
        get => ShippingDetails?.Collis.Sum(c => c.Cbm) ?? 0;
    }
}
