using SpotQuoteBooking.EventSource.Application.DTOs;

namespace SpotQuoteBooking.EventSource.Application.Interfaces;

public interface IAddressService
{
    Task UpsertAddressAsync(AddressDto addressDto);
    Task<AddressDto?> GetAddressByIdAsync(Guid addressId);
}
