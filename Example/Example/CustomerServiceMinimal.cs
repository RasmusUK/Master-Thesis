using EventSourcingFramework.Core.Interfaces;

namespace Example;

public class CustomerServiceMinimal
{
    private readonly IRepository<Customer> repository;

    public CustomerServiceMinimal(IRepository<Customer> repository)
    {
        this.repository = repository;
    }

    public async Task<Guid> CreateCustomerAsync(string name)
    {
        var customer = new Customer { Name = name };
        await repository.CreateAsync(customer);
        return customer.Id;
    }

    public Task<Customer?> GetCustomerAsync(Guid id)
    {
        return repository.ReadByIdAsync(id);
    }
}
