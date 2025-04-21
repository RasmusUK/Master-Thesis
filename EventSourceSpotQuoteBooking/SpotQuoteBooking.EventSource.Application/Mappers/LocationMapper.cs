using SpotQuoteBooking.EventSource.Application.DTOs;
using SpotQuoteBooking.EventSource.Core.AggregateRoots;

namespace SpotQuoteBooking.EventSource.Application.Mappers;

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
        };
    }
}
