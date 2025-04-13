using EventSource.Core.Exceptions;
using EventSource.Core.Interfaces;
using SpotQuoteBooking.EventSource.Application.DTOs;
using SpotQuoteBooking.EventSource.Application.Interfaces;
using SpotQuoteBooking.EventSource.Application.Mappers;
using SpotQuoteBooking.EventSource.Core.AggregateRoots;

namespace SpotQuoteBooking.EventSource.Application;

public class AddressService : IAddressService
{
    private readonly IRepository<Address> addressRepository;
    private readonly ICountryService countryService;

    public AddressService(IRepository<Address> addressRepository, ICountryService countryService)
    {
        this.addressRepository = addressRepository;
        this.countryService = countryService;
    }

    public async Task<Guid> CreateIfNotExistsAsync(AddressDto addressDto)
    {
        Address? existing;

        if (addressDto.Id == default)
            existing = await addressRepository.ReadByFilterAsync(a =>
                a.CompanyName == addressDto.CompanyName
                && a.Email == addressDto.Email
                && a.Phone == addressDto.Phone
                && a.Attention == addressDto.Attention
                && a.CountryId == addressDto.Country.Id
                && a.ZipCode == addressDto.ZipCode
                && a.AddressLine1 == addressDto.AddressLine1
                && a.AddressLine2 == addressDto.AddressLine2
                && a.City == addressDto.City
            );
        else
            existing = await addressRepository.ReadByIdAsync(addressDto.Id);

        if (existing is not null)
            return existing.Id;

        addressDto.Id = addressDto.Id == default ? Guid.NewGuid() : addressDto.Id;

        var addressDomain = addressDto.ToDomain();
        await addressRepository.CreateAsync(addressDomain);

        return addressDomain.Id;
    }

    public async Task<AddressDto?> GetAddressByIdAsync(Guid addressId)
    {
        var address = await addressRepository.ReadByIdAsync(addressId);
        if (address is null)
            return null;

        var country = await countryService.GetCountryByIdAsync(address.CountryId);
        if (country is null)
            throw new NotFoundException(
                $"Country with id '{address.CountryId}' not found for Address with id '{addressId}'."
            );

        return address.ToDto(country);
    }

    public async Task<IReadOnlyCollection<AddressDto>> GetAllAddressesAsync()
    {
        var addresses = await addressRepository.ReadAllAsync();
        var dtos = await Task.WhenAll(addresses.Select(FillAddressDtoAsync));
        return dtos;
    }

    private async Task<AddressDto> FillAddressDtoAsync(Address address)
    {
        var country = await countryService.GetCountryByIdAsync(address.CountryId);
        if (country is null)
            throw new NotFoundException($"Country with id '{address.CountryId}' not found.");

        return address.ToDto(country);
    }
}
