using SpotQuoteBooking.EventSource.Application.DTOs;

namespace SpotQuoteBooking.EventSource.Application.Interfaces;

public interface IAddressService
{
    Task<Guid> CreateIfNotExistsAsync(AddressDto addressDto);
    Task<AddressDto?> GetAddressByIdAsync(Guid addressId);
    Task<IReadOnlyCollection<AddressDto>> GetAllAddressesAsync();
}
