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

    public async Task UpsertAddressAsync(AddressDto addressDto)
    {
        var addressDomain = addressDto.ToDomain();
        var existingAddress = await addressRepository.ReadByIdAsync(addressDomain.Id);
        if (existingAddress is null)
            await addressRepository.CreateAsync(addressDomain);
        else
            await addressRepository.UpdateAsync(addressDomain);
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
}
