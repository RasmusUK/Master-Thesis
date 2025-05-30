using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.AggregateRoots;

namespace SpotQuoteApp.Application.Mappers;

public static class AddressMapper
{
    public static AddressDto ToDto(this Address address, CountryDto country)
    {
        return new AddressDto
        {
            Id = address.Id,
            City = address.City,
            AddressLine1 = address.AddressLine1,
            ZipCode = address.ZipCode,
            Country = country,
            AddressLine2 = address.AddressLine2,
            Attention = address.Attention,
            Airport = address.Airport,
            CompanyName = address.CompanyName,
            Email = address.Email,
            Phone = address.Phone,
            Port = address.Port,
            ConcurrencyVersion = address.ConcurrencyVersion,
        };
    }

    public static Address ToDomain(this AddressDto addressDto)
    {
        return new Address(
            addressDto.CompanyName,
            addressDto.Country.Id,
            addressDto.City,
            addressDto.ZipCode,
            addressDto.AddressLine1,
            addressDto.AddressLine2,
            addressDto.Email,
            addressDto.Phone,
            addressDto.Attention,
            addressDto.Port,
            addressDto.Airport
        )
        {
            Id = addressDto.Id,
            ConcurrencyVersion = addressDto.ConcurrencyVersion,
        };
    }
}
