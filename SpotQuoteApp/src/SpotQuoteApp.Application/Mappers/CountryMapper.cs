using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.DomainObjects;

namespace SpotQuoteApp.Application.Mappers;

public static class CountryMapper
{
    public static CountryDto ToDto(this Country country)
    {
        return new CountryDto
        {
            Id = country.Id,
            Name = country.Name,
            Code = country.Code,
            ConcurrencyVersion = country.ConcurrencyVersion,
        };
    }

    public static Country ToDomain(this CountryDto countryDto)
    {
        return new Country(countryDto.Name, countryDto.Code)
        {
            Id = countryDto.Id,
            ConcurrencyVersion = countryDto.ConcurrencyVersion,
        };
    }
}
