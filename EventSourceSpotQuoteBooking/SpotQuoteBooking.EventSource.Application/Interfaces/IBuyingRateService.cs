using SpotQuoteBooking.EventSource.Application.DTOs;
using SpotQuoteBooking.EventSource.Core.ValueObjects.Enums;

namespace SpotQuoteBooking.EventSource.Application.Interfaces;

public interface IBuyingRateService
{
    Task UpsertBuyingRatesAsync(SpotQuoteDto spotQuoteDto);
    Task<IReadOnlyCollection<BuyingRateDto>> SearchBuyingRatesAsync(
        AddressDto addressFrom,
        AddressDto addressTo,
        TransportMode transportMode,
        Supplier supplier,
        SupplierService supplierService,
        ForwarderService forwarderService
    );
}
