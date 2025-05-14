using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Application.Interfaces;

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
