using SpotQuoteBooking.EventSource.Application.DTOs;

namespace SpotQuoteBooking.EventSource.Application.Interfaces;

public interface ICustomerService
{
    Task<IReadOnlyCollection<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId);
    Task AddCustomerAsync(CustomerDto customer);

    Task DeleteCustomerAsync(CustomerDto customer);

    Task AddUserAsync(Guid customerId, UserDto user);

    Task DeleteUserAsync(Guid customerId, Guid userId);
}
