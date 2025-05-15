using EventSourcingFramework.Application.Abstractions.ApiGateway;
using EventSourcingFramework.Core.Interfaces;
using Microsoft.Extensions.Options;
using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Application.DTOs.Api.Requests;
using SpotQuoteApp.Application.DTOs.Api.Responses;
using SpotQuoteApp.Application.Interfaces;
using SpotQuoteApp.Application.Mappers;
using SpotQuoteApp.Application.Options;
using SpotQuoteApp.Core.AggregateRoots;
using SpotQuoteApp.Core.ValueObjects;
using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Application.Services;

public class BuyingRateService : IBuyingRateService
{
    private readonly IRepository<BuyingRate> buyingRateRepository;
    private readonly ILocationService locationService;
    private readonly IApiGateway apiGateway;
    private readonly string buyingRateApiUrl;

    public BuyingRateService(
        IRepository<BuyingRate> buyingRateRepository,
        ILocationService locationService, IApiGateway apiGateway, IOptions<MockApiOptions> options)
    {
        this.buyingRateRepository = buyingRateRepository;
        this.locationService = locationService;
        this.apiGateway = apiGateway;
        buyingRateApiUrl = $"{options.Value.BaseUrl}/api/buying-rates";
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
        var request = new BuyingRateRequest(supplier.Value, forwarderService.Name, supplierService.Name,
            addressFrom.Country.Code, addressTo.Country.Code, transportMode.ToString());
        
        var fetchedRates = (await apiGateway.PostAsync<BuyingRateRequest, BuyingRateResponseBatch>(
            url: buyingRateApiUrl,
            body: request
        )).Rates.Select(r => new BuyingRateDto
        {
            TransportMode = TransportMode.FromString(r.TransportMode),
            Supplier = supplier,
            SupplierService = supplierService,
            ForwarderService = forwarderService,
            ValidFrom = r.ValidFrom,
            ValidUntil = r.ValidTo,
            Origin = new LocationDto
            {
                Country = addressFrom.Country,
                Code = addressFrom.ZipCode,
                Name = addressFrom.City,
                Type = LocationType.ZipCode
            },
            Destination = new LocationDto
            {
                Country = addressTo.Country,
                Code = addressTo.ZipCode,
                Name = addressTo.City,
                Type = LocationType.ZipCode
            },
            SupplierCost = new SupplierCostDto
            {
                ChargeType = ChargeType.FromString(r.ChargeType),
                CostType = CostType.FromString(r.CostType),
                Value = r.Price
            }
        });
        
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
            return fetchedRates.ToList();

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
            }).Concat(fetchedRates)
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
