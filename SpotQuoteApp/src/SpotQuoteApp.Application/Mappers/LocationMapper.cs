using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.DomainObjects;

namespace SpotQuoteApp.Application.Mappers;

public static class LocationMapper
{
    public static LocationDto ToDto(this Location location, CountryDto country)
    {
        return new LocationDto
        {
            Id = location.Id,
            Code = location.Code,
            Name = location.Name,
            Country = country,
            Type = location.Type,
            ConcurrencyVersion = location.ConcurrencyVersion,
        };
    }
}
