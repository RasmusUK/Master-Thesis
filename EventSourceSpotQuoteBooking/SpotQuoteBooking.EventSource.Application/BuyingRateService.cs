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

    public async Task UpsertBuyingRatesAsync(SpotQuoteDto spotQuoteDto)
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

    public async Task<IReadOnlyCollection<BuyingRateDto>> SearchBuyingRatesAsync(
        AddressDto addressFrom,
        AddressDto addressTo,
        TransportMode transportMode,
        Supplier supplier,
        SupplierService supplierService,
        ForwarderService forwarderService
    )
    {
        LocationDto? fromLocation;
        LocationDto? toLocation;

        if (transportMode == TransportMode.Sea)
        {
            fromLocation = await locationService.SearchLocationIdAsync(
                addressFrom.Port!,
                addressFrom.Country.Code,
                LocationType.Port
            );
            toLocation = await locationService.SearchLocationIdAsync(
                addressTo.Port!,
                addressTo.Country.Code,
                LocationType.Port
            );
        }
        else if (transportMode == TransportMode.Air)
        {
            fromLocation = await locationService.SearchLocationIdAsync(
                addressFrom.Airport!,
                addressFrom.Country.Code,
                LocationType.Airport
            );
            toLocation = await locationService.SearchLocationIdAsync(
                addressTo.Airport!,
                addressTo.Country.Code,
                LocationType.Airport
            );
        }
        else
        {
            fromLocation = await locationService.SearchLocationIdAsync(
                addressFrom.ZipCode,
                addressFrom.Country.Code,
                LocationType.ZipCode
            );
            toLocation = await locationService.SearchLocationIdAsync(
                addressTo.ZipCode,
                addressTo.Country.Code,
                LocationType.ZipCode
            );
        }

        if (fromLocation is null || toLocation is null)
            return new List<BuyingRateDto>();

        var buyingRates = await buyingRateRepository.ReadAllByFilterAsync(b =>
            b.OriginLocationId == fromLocation.Id
            && b.DestinationLocationId == toLocation.Id
            && b.ValidUntil >= DateTime.UtcNow
            && b.Supplier == supplier
            && b.SupplierService == supplierService
            && b.ForwarderService == forwarderService
        );

        return buyingRates
            .Select(b => new BuyingRateDto
            {
                Id = b.Id,
                TransportMode = b.TransportMode,
                Supplier = b.Supplier,
                SupplierService = b.SupplierService,
                ForwarderService = b.ForwarderService,
                Origin = fromLocation,
                Destination = toLocation,
                ValidFrom = b.ValidFrom,
                ValidUntil = b.ValidUntil,
                SupplierCost = b.SupplierCost.ToDto(),
            })
            .ToList();
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
