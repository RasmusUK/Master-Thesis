using EventSourcingFramework.Core.Exceptions;
using EventSourcingFramework.Core.Interfaces;
using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Application.Interfaces;
using SpotQuoteApp.Application.Mappers;
using SpotQuoteApp.Core.AggregateRoots;

namespace SpotQuoteApp.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> customerRepository;
    private readonly IRepository<SpotQuote> spotQuoteRepository;

    public CustomerService(
        IRepository<Customer> customerRepository,
        IRepository<SpotQuote> spotQuoteRepository
    )
    {
        this.customerRepository = customerRepository;
        this.spotQuoteRepository = spotQuoteRepository;
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

    public Task AddCustomerAsync(CustomerDto customer) =>
        customerRepository.CreateAsync(customer.ToDomain());

    public async Task DeleteCustomerAsync(CustomerDto customer)
    {
        var spotQuotes = await spotQuoteRepository.ReadAllProjectionsByFilterAsync(
            x => x.Id,
            x => x.CustomerId == customer.Id
        );

        if (spotQuotes.Any())
            throw new InvalidOperationException(
                $"Cannot delete customer '{customer.Id}' because there are spot quotes associated with this customer."
            );

        await customerRepository.DeleteAsync(customer.ToDomain());
    }

    public async Task AddUserAsync(Guid customerId, UserDto user)
    {
        var customer = await customerRepository.ReadByIdAsync(customerId);
        if (customer is null)
            throw new NotFoundException($"Customer with id '{customerId}' not found.");

        var userDomain = user.ToDomain();
        customer.Users.Add(userDomain);

        await customerRepository.UpdateAsync(customer);
    }

    public async Task DeleteUserAsync(Guid customerId, Guid userId)
    {
        var customer = await customerRepository.ReadByIdAsync(customerId);
        if (customer is null)
            throw new NotFoundException($"Customer with id '{customerId}' not found.");

        var spotQuotes = (
            await spotQuoteRepository.ReadAllProjectionsByFilterAsync(
                x => new { x.Id, x.MailOptions },
                x => x.CustomerId == customerId
            )
        ).Where(x => x.MailOptions.RecipientUserIds.Contains(userId));

        if (spotQuotes.Any())
            throw new InvalidOperationException(
                $"Cannot delete user '{userId}' from customer '{customerId}' because there are spot quotes associated with this user."
            );

        var user = customer.Users.FirstOrDefault(u => u.Id == userId);
        if (user is null)
            throw new NotFoundException(
                $"User with id '{userId}' not found in customer '{customerId}'."
            );

        customer.Users.Remove(user);
        await customerRepository.UpdateAsync(customer);
    }
}
