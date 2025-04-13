using EventSource.Core;
using SpotQuoteBooking.EventSource.Core.ValueObjects.Enums;

namespace SpotQuoteBooking.EventSource.Core.AggregateRoots;

public class BuyingRate : Entity
{
    public BuyingRate(
        TransportMode transportMode,
        Supplier supplier,
        SupplierService supplierService,
        ForwarderService forwarderService,
        Guid originLocation,
        Guid destinationLocation,
        DateTime validFrom,
        DateTime validUntil,
        double price
    )
    {
        TransportMode = transportMode;
        Supplier = supplier;
        SupplierService = supplierService;
        ForwarderService = forwarderService;
        OriginLocationId = originLocation;
        DestinationLocationId = destinationLocation;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        Price = price;
    }

    public TransportMode TransportMode { get; set; }
    public Supplier Supplier { get; set; }
    public SupplierService SupplierService { get; set; }
    public ForwarderService ForwarderService { get; set; }
    public Guid OriginLocationId { get; set; }
    public Guid DestinationLocationId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public double Price { get; set; }
}
