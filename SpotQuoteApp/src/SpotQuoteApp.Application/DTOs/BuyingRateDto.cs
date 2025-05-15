using SpotQuoteApp.Core.AggregateRoots;
using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Application.DTOs;

public class BuyingRateDto
{
    public Guid Id { get; set; }
    public int ConcurrencyVersion { get; set; }
    public TransportMode TransportMode { get; set; }
    public Supplier Supplier { get; set; }
    public SupplierService SupplierService { get; set; }
    public ForwarderService ForwarderService { get; set; }
    public LocationDto Origin { get; set; }
    public LocationDto Destination { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public SupplierCostDto SupplierCost { get; set; }
}
