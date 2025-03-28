using EventSource.Core;
using SpotQuoteBooking.Shared;

namespace SpotQuoteBooking.EventSource.Core;

public class SpotQuoteBooking : Entity
{
    public Address AddressFrom { get; private set; }
    public Address AddressTo { get; private set; }
    public Direction Direction { get; private set; }
    public TransportMode TransportMode { get; private set; }
    public Incoterm Incoterm { get; private set; }
    public ShippingDetails ShippingDetails { get; private set; }
    public DateTime ValidUntil { get; private set; }

    private void Apply(CreateSpotQuoteBookingEvent e)
    {
        AddressFrom = e.AddressFrom;
        AddressTo = e.AddressTo;
        Direction = e.Direction;
        TransportMode = e.TransportMode;
        Incoterm = e.Incoterm;
        ShippingDetails = e.ShippingDetails;
    }
}
