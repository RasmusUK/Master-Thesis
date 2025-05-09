using EventSourcingFramework.Persistence;
using SpotQuoteApp.Core.ValueObjects;
using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Core.AggregateRoots;

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
        SupplierCost supplierCost
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
        SupplierCost = supplierCost;
    }

    public TransportMode TransportMode { get; set; }
    public Supplier Supplier { get; set; }
    public SupplierService SupplierService { get; set; }
    public ForwarderService ForwarderService { get; set; }
    public Guid OriginLocationId { get; set; }
    public Guid DestinationLocationId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public SupplierCost SupplierCost { get; set; }
}
