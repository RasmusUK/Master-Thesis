using SpotQuoteBooking.EventSource.Application.DTOs;
using SpotQuoteBooking.EventSource.Core.AggregateRoots;

namespace SpotQuoteBooking.EventSource.Application.Mappers;

public static class CountryMapper
{
    public static CountryDto ToDto(this Country country)
    {
        return new CountryDto
        {
            Id = country.Id,
            Name = country.Name,
            Code = country.Code,
        };
    }

    public static Country ToDomain(this CountryDto countryDto)
    {
        return new Country(countryDto.Name, countryDto.Code) { Id = countryDto.Id };
    }
}
