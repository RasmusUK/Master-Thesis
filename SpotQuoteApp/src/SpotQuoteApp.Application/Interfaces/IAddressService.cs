using SpotQuoteApp.Application.DTOs;

namespace SpotQuoteApp.Application.Interfaces;

public interface IAddressService
{
    Task<Guid> CreateIfNotExistsAsync(AddressDto addressDto);
    Task<AddressDto?> GetAddressByIdAsync(Guid addressId);
    Task<IReadOnlyCollection<AddressDto>> GetAllAddressesAsync();
}
