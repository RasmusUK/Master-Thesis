using EventSource.Core.Interfaces;
using SpotQuoteBooking.EventSource.Application.DTOs;
using SpotQuoteBooking.EventSource.Application.Interfaces;
using SpotQuoteBooking.EventSource.Application.Mappers;
using SpotQuoteBooking.EventSource.Core.AggregateRoots;
using SpotQuoteBooking.EventSource.Core.Exceptions;
using SpotQuoteBooking.EventSource.Core.ValueObjects.Enums;

namespace SpotQuoteBooking.EventSource.Application;

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
