using EventSource.Core.Interfaces;
using SpotQuoteBooking.EventSource.Application.DTOs;
using SpotQuoteBooking.EventSource.Application.Interfaces;
using SpotQuoteBooking.EventSource.Application.Mappers;
using SpotQuoteBooking.EventSource.Core.AggregateRoots;
using SpotQuoteBooking.EventSource.Core.ValueObjects.Enums;

namespace SpotQuoteBooking.EventSource.Application;

public class BuyingRateService : IBuyingRateService
{
    private readonly IRepository<BuyingRate> buyingRateRepository;
    private readonly ILocationService locationService;

    public BuyingRateService(
        IRepository<BuyingRate> buyingRateRepository,
        ILocationService locationService
    )
    {
        this.buyingRateRepository = buyingRateRepository;
        this.locationService = locationService;
    }

    public async Task CreateBuyingRatesIfNotExistsAsync(SpotQuoteDto spotQuoteDto)
    {
        foreach (var quoteDto in spotQuoteDto.Quotes)
        {
            foreach (var supplierCostDto in quoteDto.Costs.Select(c => c.SupplierCost))
            {
                var addressFromId = await CreateLocationIfNotExistsAsync(
                    spotQuoteDto.AddressFrom,
                    spotQuoteDto.TransportMode
                );
                var addressToId = await CreateLocationIfNotExistsAsync(
                    spotQuoteDto.AddressTo,
                    spotQuoteDto.TransportMode
                );

                var supplierCost = supplierCostDto.ToDomain();

                var buyingRate = new BuyingRate(
                    spotQuoteDto.TransportMode,
                    quoteDto.Supplier,
                    quoteDto.SupplierService,
                    quoteDto.ForwarderService,
                    addressFromId,
                    addressToId,
                    DateTime.UtcNow,
                    spotQuoteDto.ValidUntil!.Value,
                    supplierCost
                );

                var existingBuyingRate = await buyingRateRepository.ReadByFilterAsync(b =>
                    b.ForwarderService == quoteDto.ForwarderService
                    && b.SupplierService == quoteDto.SupplierService
                    && b.Supplier == quoteDto.Supplier
                    && b.OriginLocationId == addressFromId
                    && b.DestinationLocationId == addressToId
                    && b.SupplierCost == supplierCost
                );

                if (existingBuyingRate is null)
                {
                    await buyingRateRepository.CreateAsync(buyingRate);
                    continue;
                }

                if (existingBuyingRate.ValidUntil >= buyingRate.ValidUntil)
                    continue;

                existingBuyingRate.ValidUntil = buyingRate.ValidUntil;
                await buyingRateRepository.UpdateAsync(existingBuyingRate);
            }
        }
    }

    private Task<Guid> CreateLocationIfNotExistsAsync(
        AddressDto address,
        TransportMode transportMode
    )
    {
        var code = string.Empty;
        var name = string.Empty;
        var countryCode = address.Country.Code;
        var locationType = LocationType.ZipCode;

        if (transportMode == TransportMode.Air)
        {
            code = address.Airport;
            name = $"Aiport {address.Airport}";
            locationType = LocationType.Airport;
        }
        else if (transportMode == TransportMode.Sea)
        {
            code = address.Port;
            name = $"Port {address.Port}";
            locationType = LocationType.Port;
        }
        else
        {
            code = address.ZipCode;
            name = $"ZipCode {address.ZipCode}";
        }

        return locationService.CreateLocationIfNotExistsAsync(
            code,
            name,
            countryCode,
            locationType
        );
    }
}
