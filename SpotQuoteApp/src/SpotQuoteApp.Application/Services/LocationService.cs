using EventSourcingFramework.Core.Interfaces;
using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Application.Interfaces;
using SpotQuoteApp.Application.Mappers;
using SpotQuoteApp.Core.DomainObjects;
using SpotQuoteApp.Core.Exceptions;
using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Application.Services;

public class LocationService : ILocationService
{
    private readonly IRepository<Location> locationRepository;
    private readonly ICountryService countryService;

    public LocationService(IRepository<Location> locationRepository, ICountryService countryService)
    {
        this.locationRepository = locationRepository;
        this.countryService = countryService;
    }

    public async Task<Guid> CreateLocationIfNotExistsAsync(
        string code,
        string name,
        string countryCode,
        LocationType locationType
    )
    {
        var country = await countryService.GetCountryByCodeAsync(countryCode);
        if (country is null)
            throw new NotFoundException($"Country with code {countryCode} not found.");

        var existing = await locationRepository.ReadByFilterAsync(l =>
            l.Code == code && l.Name == name && l.CountryId == country.Id && l.Type == locationType
        );

        if (existing is not null)
            return existing.Id;

        var location = new Location(code, name, country.Id, locationType);
        await locationRepository.CreateAsync(location);
        return location.Id;
    }

    public async Task<LocationDto?> SearchLocationIdAsync(
        string code,
        string countryCode,
        LocationType locationType
    )
    {
        var country = await countryService.GetCountryByCodeAsync(countryCode);
        if (country is null)
            return null;

        var location = await locationRepository.ReadByFilterAsync(l =>
            l.Code == code && l.CountryId == country.Id && l.Type == locationType
        );

        return location?.ToDto(country);
    }
}
