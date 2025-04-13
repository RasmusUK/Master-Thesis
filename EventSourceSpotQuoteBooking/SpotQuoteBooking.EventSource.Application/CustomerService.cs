using EventSource.Core.Interfaces;
using SpotQuoteBooking.EventSource.Application.DTOs;
using SpotQuoteBooking.EventSource.Application.Interfaces;
using SpotQuoteBooking.EventSource.Application.Mappers;
using SpotQuoteBooking.EventSource.Core.AggregateRoots;

namespace SpotQuoteBooking.EventSource.Application;

public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> customerRepository;

    public CustomerService(IRepository<Customer> customerRepository)
    {
        this.customerRepository = customerRepository;
    }

    public async Task<IReadOnlyCollection<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await customerRepository.ReadAllAsync();
        return customers.Select(CustomerMapper.ToDto).ToList();
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId)
    {
        var customer = await customerRepository.ReadByIdAsync(customerId);
        return customer?.ToDto();
    }
}
