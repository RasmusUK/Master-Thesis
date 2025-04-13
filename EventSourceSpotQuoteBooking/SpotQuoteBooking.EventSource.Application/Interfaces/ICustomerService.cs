using SpotQuoteBooking.EventSource.Application.DTOs;

namespace SpotQuoteBooking.EventSource.Application.Interfaces;

public interface ICustomerService
{
    Task<IReadOnlyCollection<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId);
}
